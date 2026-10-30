using System.Data;
using B2B.Core;
using Microsoft.EntityFrameworkCore;

namespace B2B.Data;

public class AuthService(B2BContext db)
{
    /// <summary>Authenticates by user name or e-mail and applies the lockout policy.</summary>
    public (LoginResult Result, User? User) Login(string userNameOrEmail, string password, string requiredRole)
    {
        var key = (userNameOrEmail ?? "").Trim();
        var user = db.Users.Include(u => u.Agent).FirstOrDefault(u => u.UserName == key || u.Email == key);
        var result = LockoutPolicy.Evaluate(user, password ?? "", requiredRole);
        if (user is not null) db.SaveChanges(); // persist FailedLoginCount / Lock
        return (result, result == LoginResult.Success ? user : null);
    }
}

public record OrderLine(int ItemID, int Quantity);

public class OrderService(B2BContext db)
{
    /// <summary>
    /// Places a master-detail order in ONE database transaction:
    /// 1) deduct stock for every line with a conditional UPDATE (StockQuantity >= qty),
    /// 2) insert the Order and its OrderDetails.
    /// If any item has insufficient stock the transaction is rolled back and nothing is saved.
    /// </summary>
    public Order PlaceOrder(int agentId, IEnumerable<OrderLine> lines)
    {
        var merged = lines.Where(l => l.Quantity != 0)
                          .GroupBy(l => l.ItemID)
                          .Select(g => new OrderLine(g.Key, g.Sum(l => l.Quantity)))
                          .ToList();
        if (merged.Count == 0) throw new OrderException("The order must contain at least one item.");
        if (merged.Any(l => l.Quantity < 0)) throw new OrderException("Quantities must be positive.");
        if (!db.Agents.Any(a => a.AgentID == agentId)) throw new OrderException("Unknown agent.");

        using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            var order = new Order { AgentID = agentId, OrderDate = DateTime.Now, OrderStatus = OrderStatuses.Pending };
            foreach (var line in merged)
            {
                var item = db.Items.AsNoTracking().FirstOrDefault(i => i.ItemID == line.ItemID)
                           ?? throw new OrderException($"Item #{line.ItemID} does not exist.");

                // Atomic check-and-deduct: only succeeds if enough stock is still available.
                int updated = db.Items
                    .Where(i => i.ItemID == line.ItemID && i.StockQuantity >= line.Quantity)
                    .ExecuteUpdate(s => s.SetProperty(i => i.StockQuantity, i => i.StockQuantity - line.Quantity));
                if (updated == 0)
                    throw new OrderException($"Insufficient stock for '{item.ItemName}': requested {line.Quantity}, available {item.StockQuantity}.");

                order.OrderDetails.Add(new OrderDetail { ItemID = item.ItemID, Quantity = line.Quantity, UnitAmount = item.Price });
            }
            db.Orders.Add(order);
            db.SaveChanges();
            tx.Commit();
            return order;
        }
        catch
        {
            tx.Rollback();
            db.ChangeTracker.Clear();
            throw;
        }
    }

    /// <summary>Admin status change. Cancelling an order returns its quantities to stock (same transaction).</summary>
    public void ChangeStatus(int orderId, string newStatus)
    {
        using var tx = db.Database.BeginTransaction();
        var order = db.Orders.Include(o => o.OrderDetails).FirstOrDefault(o => o.OrderID == orderId)
                    ?? throw new OrderException($"Order #{orderId} not found.");
        if (!OrderWorkflow.CanChange(order.OrderStatus, newStatus))
            throw new OrderException($"Cannot change status from {order.OrderStatus} to {newStatus}.");
        if (newStatus == OrderStatuses.Cancelled)
            foreach (var d in order.OrderDetails)
                db.Items.Where(i => i.ItemID == d.ItemID)
                        .ExecuteUpdate(s => s.SetProperty(i => i.StockQuantity, i => i.StockQuantity + d.Quantity));
        order.OrderStatus = newStatus;
        db.SaveChanges();
        tx.Commit();
    }

    public Order? GetOrder(int orderId) =>
        db.Orders.AsNoTracking().Include(o => o.Agent).Include(o => o.OrderDetails).ThenInclude(d => d.Item)
          .FirstOrDefault(o => o.OrderID == orderId);

    public List<Order> GetOrders(int? agentId = null, string? status = null) =>
        db.Orders.AsNoTracking().Include(o => o.Agent).Include(o => o.OrderDetails).ThenInclude(d => d.Item)
          .Where(o => (agentId == null || o.AgentID == agentId) && (status == null || o.OrderStatus == status))
          .OrderByDescending(o => o.OrderDate).ToList();
}

public record BestSellerRow(int ItemID, string ItemName, string Size, int QuantitySold, decimal Revenue);
public record AgentRevenueRow(int AgentID, string AgentName, int Orders, decimal Revenue);

/// <summary>Dashboard queries (cancelled orders are excluded from sales figures).</summary>
public class ReportService(B2BContext db)
{
    public List<BestSellerRow> BestSelling(DateTime from, DateTime to, int top = 10) =>
        db.OrderDetails.AsNoTracking()
          .Where(d => d.Order!.OrderStatus != OrderStatuses.Cancelled && d.Order.OrderDate >= from && d.Order.OrderDate < to.Date.AddDays(1))
          .GroupBy(d => new { d.ItemID, d.Item!.ItemName, d.Item.Size })
          .Select(g => new BestSellerRow(g.Key.ItemID, g.Key.ItemName, g.Key.Size, g.Sum(d => d.Quantity), g.Sum(d => d.Quantity * d.UnitAmount)))
          .AsEnumerable().OrderByDescending(r => r.QuantitySold).Take(top).ToList();

    public List<Item> LowStock(int threshold) =>
        db.Items.AsNoTracking().Where(i => i.StockQuantity <= threshold).OrderBy(i => i.StockQuantity).ToList();

    public List<AgentRevenueRow> RevenueByAgent(DateTime from, DateTime to, int? agentId = null) =>
        db.Orders.AsNoTracking()
          .Where(o => o.OrderStatus != OrderStatuses.Cancelled && o.OrderDate >= from && o.OrderDate < to.Date.AddDays(1)
                      && (agentId == null || o.AgentID == agentId))
          .GroupBy(o => new { o.AgentID, o.Agent!.AgentName })
          .Select(g => new AgentRevenueRow(g.Key.AgentID, g.Key.AgentName, g.Count(),
                       g.SelectMany(o => o.OrderDetails).Sum(d => d.Quantity * d.UnitAmount)))
          .AsEnumerable().OrderByDescending(r => r.Revenue).ToList();
}
