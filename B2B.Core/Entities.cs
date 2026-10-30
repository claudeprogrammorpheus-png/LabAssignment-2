using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B2B.Core;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Agent = "Agent";
}

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Pending, Processing, Shipped, Delivered, Cancelled];
}

public class Item
{
    public int ItemID { get; set; }

    [Required, StringLength(100)]
    public string ItemName { get; set; } = "";

    [Required, StringLength(30)]
    public string Size { get; set; } = "";

    [Column(TypeName = "decimal(18,2)"), Range(0, 1_000_000_000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public List<OrderDetail> OrderDetails { get; set; } = new();
}

public class Agent
{
    public int AgentID { get; set; }

    [Required, StringLength(100)]
    public string AgentName { get; set; } = "";

    [Required, StringLength(200)]
    public string Address { get; set; } = "";

    [Required, StringLength(20), Phone]
    public string Phone { get; set; } = "";

    public List<Order> Orders { get; set; } = new();
}

[Table("Order")]
public class Order
{
    public int OrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public int AgentID { get; set; }

    [Required, StringLength(20)]
    public string OrderStatus { get; set; } = OrderStatuses.Pending;

    public Agent? Agent { get; set; }
    public List<OrderDetail> OrderDetails { get; set; } = new();

    [NotMapped]
    public decimal TotalAmount => OrderCalculator.Total(OrderDetails);
}

public class OrderDetail
{
    public int ID { get; set; }
    public int OrderID { get; set; }
    public int ItemID { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    /// <summary>Unit price at the time of ordering (prices may change later).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitAmount { get; set; }

    public Order? Order { get; set; }
    public Item? Item { get; set; }

    [NotMapped]
    public decimal LineTotal => Quantity * UnitAmount;
}

public class User
{
    public int UserID { get; set; }

    [Required, StringLength(50)]
    public string UserName { get; set; } = "";

    [Required, StringLength(100), EmailAddress]
    public string Email { get; set; } = "";

    /// <summary>PBKDF2 hash ("iterations.salt.hash"), never the plain password.</summary>
    [Required, StringLength(200)]
    public string Password { get; set; } = "";

    /// <summary>True when the account is locked (manually or after too many failed logins).</summary>
    public bool Lock { get; set; }

    [Required, StringLength(10)]
    public string Role { get; set; } = Roles.Agent;

    /// <summary>Consecutive failed logins; reset on success.</summary>
    public int FailedLoginCount { get; set; }

    /// <summary>Links an Agent-role account to its distributor (null for Admins).</summary>
    public int? AgentID { get; set; }
    public Agent? Agent { get; set; }
}
