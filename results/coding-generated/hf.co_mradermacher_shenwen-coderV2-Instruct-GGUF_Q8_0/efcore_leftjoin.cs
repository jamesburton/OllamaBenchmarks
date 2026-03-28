using Microsoft.EntityFrameworkCore;
using System.Linq;

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasJsonConversion();

        modelBuilder.Entity<Customer>()
            .Property(c => c.Address)
            .HasJsonConversion();
    }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }

    public virtual Customer? Customer { get; set; }
}

public class OrderDto : IOrderDto
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public string CustomerName { get; set; }

    public OrderDto(int orderId, decimal total, string customerName)
    {
        Id = orderId;
        Total = total;
        CustomerName = customerName;
    }
}

public static class OrderQueries
{
    public static async Task<List<OrderDto>> GetOrdersWithCustomers(AppDbContext db)
    {
        var orders = await db.Orders
            .Where(o => o.Status == "Active")
            .ToListAsync();

        return orders.Select(order =>
        {
            var customer = order.Customer;
            if (customer != null)
                return new OrderDto(order.Id, order.Total, customer.Name);
            else
                return new OrderDto(order.Id, order.Total, "Unknown");
        }).ToList();
    }
}