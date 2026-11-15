using B2B.Core;
using Microsoft.EntityFrameworkCore;

namespace B2B.Data;

public class B2BContext(DbContextOptions<B2BContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Item>(e =>
        {
            e.ToTable("Item", t =>
            {
                t.HasCheckConstraint("CK_Item_Stock", "[StockQuantity] >= 0");
                t.HasCheckConstraint("CK_Item_Price", "[Price] >= 0");
            });
            e.HasIndex(i => i.ItemName);
        });

        b.Entity<Agent>(e =>
        {
            e.ToTable("Agent");
            e.HasIndex(a => a.Phone).IsUnique();
        });

        b.Entity<Order>(e =>
        {
            e.ToTable("Order", t => t.HasCheckConstraint("CK_Order_Status",
                "[OrderStatus] IN ('Pending','Processing','Shipped','Delivered','Cancelled')"));
            // An agent with order history cannot be deleted (keeps sales history consistent).
            e.HasOne(o => o.Agent).WithMany(a => a.Orders).HasForeignKey(o => o.AgentID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(o => o.OrderDate);
        });

        b.Entity<OrderDetail>(e =>
        {
            e.ToTable("OrderDetail", t => t.HasCheckConstraint("CK_OrderDetail_Qty", "[Quantity] > 0"));
            // Deleting an order removes its lines (cascade); items with order history cannot be deleted.
            e.HasOne(d => d.Order).WithMany(o => o.OrderDetails).HasForeignKey(d => d.OrderID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.Item).WithMany(i => i.OrderDetails).HasForeignKey(d => d.ItemID).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<User>(e =>
        {
            e.ToTable("Users", t => t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Admin','Agent')"));
            e.HasIndex(u => u.UserName).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            // Deleting an agent's company removes its login accounts.
            e.HasOne(u => u.Agent).WithMany().HasForeignKey(u => u.AgentID).OnDelete(DeleteBehavior.Cascade);
        });

        SeedData.Apply(b);
    }
}
