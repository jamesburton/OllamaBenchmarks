using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
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
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();
}

public record OrderDto(int OrderId, decimal Total, string CustomerName);

public static class OrderQueries
{
    public static async Task<List<OrderDto>> GetOrdersWithCustomersAsync(AppDbContext db)
    {
        return await db.Orders
                        .LeftJoin(
                            db.Customers,
                            order => order.CustomerId,
                            customer => customer.Id,
                            (order, customer) => new OrderDto(
                                OrderId = order.Id,
                                Total = order.Total,
                                CustomerName = customer == null ? "Unknown" : customer.Name))
                        .ToListAsync();
    }
}