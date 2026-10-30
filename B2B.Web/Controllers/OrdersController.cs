using B2B.Core;
using B2B.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2B.Web.Controllers;

[Authorize(Roles = Roles.Agent)]
public class OrdersController(OrderService orders) : Controller
{
    private int AgentId => int.Parse(User.FindFirst("AgentID")!.Value);

    /// <summary>"My Orders": past orders and statuses of the logged-in agent (Task 3.3).</summary>
    public IActionResult Index(string? status)
    {
        ViewBag.Status = status;
        return View(orders.GetOrders(AgentId, string.IsNullOrEmpty(status) ? null : status));
    }

    public IActionResult Details(int id) => OwnOrder(id) is { } o ? View(o) : NotFound();

    /// <summary>Printable invoice (HTML + CSS print media; the browser can also "Save as PDF").</summary>
    public IActionResult Invoice(int id) => OwnOrder(id) is { } o ? View(o) : NotFound();

    // An agent may only see its own orders.
    private Order? OwnOrder(int id) => orders.GetOrder(id) is { } o && o.AgentID == AgentId ? o : null;
}
