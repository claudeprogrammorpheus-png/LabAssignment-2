using B2B.Core;
using B2B.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2B.Web.Controllers;

public class OrderLineInput
{
    public int ItemID { get; set; }
    public int Quantity { get; set; }
}

[Authorize(Roles = Roles.Agent)]
public class CatalogController(B2BContext db, OrderService orders) : Controller
{
    /// <summary>Catalog of items with quantity inputs; the agent builds a multi-line order on this page.</summary>
    public IActionResult Index(string? q, string? stock)
    {
        var items = db.Items.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            items = items.Where(i => i.ItemName.Contains(q) || i.Size.Contains(q));
        if (stock == "in") items = items.Where(i => i.StockQuantity > 0);
        ViewBag.Q = q;
        ViewBag.Stock = stock;
        return View(items.OrderBy(i => i.ItemName).ToList());
    }

    /// <summary>Places the master-detail order (Order + OrderDetails + stock deduction in one transaction).</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult PlaceOrder(List<OrderLineInput> lines)
    {
        int agentId = int.Parse(User.FindFirst("AgentID")!.Value);
        try
        {
            var order = orders.PlaceOrder(agentId, lines.Select(l => new OrderLine(l.ItemID, l.Quantity)));
            TempData["Success"] = $"Order #{order.OrderID} has been placed successfully. Total: {order.TotalAmount:N0} VND.";
            return RedirectToAction("Details", "Orders", new { id = order.OrderID });
        }
        catch (OrderException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
