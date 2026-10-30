using B2B.Core;
using B2B.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.Tests;

public class OrderServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    public void Dispose() => _db.Dispose();

    // Seed: item 1 Aquafina 105,000 (stock 420), item 2 Coca-Cola 215,000 (350), item 22 Cosy 560,000 (3), item 25 Pocari (0)

    [Fact]
    public void PlaceOrder_creates_order_with_details_and_deducts_stock()
    {
        using var db = _db.New();
        var order = new OrderService(db).PlaceOrder(agentId: 1, [new(1, 10), new(2, 5)]);

        Assert.True(order.OrderID > 20);
        Assert.Equal(OrderStatuses.Pending, order.OrderStatus);
        Assert.Equal(2, order.OrderDetails.Count);
        Assert.Equal(410, _db.Stock(1));
        Assert.Equal(345, _db.Stock(2));
        using var check = _db.New();
        Assert.Equal(2, check.OrderDetails.Count(d => d.OrderID == order.OrderID));
    }

    [Fact]
    public void PlaceOrder_with_insufficient_stock_is_blocked_and_rolls_back_every_line()
    {
        using var db = _db.New();
        int ordersBefore = db.Orders.Count();

        // First line is fine, second line exceeds stock -> the whole order must fail.
        var ex = Assert.Throws<OrderException>(() => new OrderService(db).PlaceOrder(1, [new(1, 10), new(22, 50)]));

        Assert.Contains("Insufficient stock", ex.Message);
        Assert.Equal(420, _db.Stock(1));   // deduction of line 1 was rolled back
        Assert.Equal(3, _db.Stock(22));
        using var check = _db.New();
        Assert.Equal(ordersBefore, check.Orders.Count()); // no order header saved
    }

    [Fact]
    public void PlaceOrder_can_take_exactly_the_remaining_stock_but_not_one_more()
    {
        using (var db = _db.New())
            new OrderService(db).PlaceOrder(2, [new(22, 3)]);
        Assert.Equal(0, _db.Stock(22));

        using var db2 = _db.New();
        Assert.Throws<OrderException>(() => new OrderService(db2).PlaceOrder(2, [new(22, 1)]));
        Assert.Equal(0, _db.Stock(22));
    }

    [Fact]
    public void PlaceOrder_records_current_price_as_unit_amount_and_total_is_correct()
    {
        using var db = _db.New();
        var order = new OrderService(db).PlaceOrder(3, [new(1, 10), new(2, 4), new(1, 2)]); // duplicate item lines are merged

        Assert.Equal(2, order.OrderDetails.Count);
        var aqua = order.OrderDetails.Single(d => d.ItemID == 1);
        Assert.Equal(12, aqua.Quantity);
        Assert.Equal(105_000m, aqua.UnitAmount);
        Assert.Equal(12 * 105_000m + 4 * 215_000m, order.TotalAmount); // 2,120,000
        Assert.Equal(408, _db.Stock(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void PlaceOrder_rejects_empty_or_negative_quantities(int qty)
    {
        using var db = _db.New();
        Assert.Throws<OrderException>(() => new OrderService(db).PlaceOrder(1, [new(1, qty)]));
        Assert.Equal(420, _db.Stock(1));
    }

    [Fact]
    public void PlaceOrder_rejects_out_of_stock_and_unknown_items()
    {
        using var db = _db.New();
        Assert.Throws<OrderException>(() => new OrderService(db).PlaceOrder(1, [new(25, 1)]));   // stock 0
        Assert.Throws<OrderException>(() => new OrderService(db).PlaceOrder(1, [new(999, 1)]));  // no such item
        Assert.Throws<OrderException>(() => new OrderService(db).PlaceOrder(999, [new(1, 1)]));  // no such agent
    }

    [Fact]
    public void Cancelling_an_order_returns_quantities_to_stock()
    {
        int id;
        using (var db = _db.New()) id = new OrderService(db).PlaceOrder(1, [new(2, 50)]).OrderID;
        Assert.Equal(300, _db.Stock(2));

        using (var db = _db.New()) new OrderService(db).ChangeStatus(id, OrderStatuses.Cancelled);
        Assert.Equal(350, _db.Stock(2));
    }

    [Fact]
    public void Invalid_status_transition_is_rejected()
    {
        using var db = _db.New();
        var svc = new OrderService(db);
        // Order 1 is Delivered in the seed data: it cannot go back to Pending or be cancelled.
        Assert.Throws<OrderException>(() => svc.ChangeStatus(1, OrderStatuses.Pending));
        Assert.Throws<OrderException>(() => svc.ChangeStatus(1, OrderStatuses.Cancelled));
        Assert.True(OrderWorkflow.CanChange(OrderStatuses.Pending, OrderStatuses.Processing));
        Assert.False(OrderWorkflow.CanChange(OrderStatuses.Pending, OrderStatuses.Delivered));
    }

    [Fact]
    public void Database_check_constraint_prevents_negative_stock()
    {
        using var db = _db.New();
        Assert.ThrowsAny<Exception>(() =>
            db.Items.Where(i => i.ItemID == 22).ExecuteUpdate(s => s.SetProperty(i => i.StockQuantity, -1)));
        Assert.Equal(3, _db.Stock(22));
    }

    [Fact]
    public void Order_total_calculator_sums_quantity_times_unit_amount()
    {
        var lines = new[] { new OrderDetail { Quantity = 3, UnitAmount = 10_000 }, new OrderDetail { Quantity = 2, UnitAmount = 2_500.5m } };
        Assert.Equal(35_001m, OrderCalculator.Total(lines));
        Assert.Equal(0m, OrderCalculator.Total(Array.Empty<OrderDetail>()));
    }
}
