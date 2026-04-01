using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public Customer Customer { get; set; } = default!;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => _orders;
    public DbSet<Customer> Customers => _customers;

    private DbSet<Order> _orders;
    private DbSet<Customer> _customers;
}

public record OrderDto(int OrderId, decimal Total, string CustomerName) { }

public static class OrderQueries
{
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        return db.Orders
            .FromEntity()
            .SelectMany(o => o.Customer
                .Where(c => c.Id == o.CustomerId)               // join condition
                .Select(c => c.Name)
                .FirstOrDefault())                            // "Unknown" if no match
            .Select(o => new OrderDto(o.Id, o.Total,                     // fallback
                         customerName ?? "Unknown"))                // fallback if query fails
            .ToList();
    }
}