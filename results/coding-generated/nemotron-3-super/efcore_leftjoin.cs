using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

public record OrderDto(int OrderId, decimal Total, string CustomerName);

public static class OrderQueries
{
    public static async Task<List<OrderDto>> GetOrdersWithCustomers(AppDbContext db)
    {
        return await db.Orders
            .LeftJoin(
                db.Customers,
                o => o.CustomerId,
                c => c.Id,
                (order, customer) => new OrderDto(
                    order.Id,
                    order.Total,
                    customer == null ? "Unknown" : customer.Name)
            )
            .ToListAsync();
    }
}