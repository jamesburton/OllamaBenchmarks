using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class Customer : IClassifiable
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ValueType ClassId => Id;
}

public class Order : IClassifiable
{
    public int Id { get; set; } = 0;
    public int CustomerId { get; set; } = 0;
    public decimal Total { get; set; } = 0m;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public ValueType ClassId => Id;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<Customer> Customers { get; set; } = default!;
}

public record OrderDto(int OrderId, decimal Total, string CustomerName);

public static class OrderQueries
{
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        return db.Orders
                 .LeftJoin(
                     db.Customers,
                     o => o.CustomerId,
                     c => c.Id,
                     (o, c) =>
                     new { o.OrderId = o.Id,
                           o.Total,
                           CustomerName = c == null ? "Unknown" : c.Name })
                 .Select(x => new OrderDto(x.OrderId, x.Total, x.CustomerName))
                 .ToList();
    }
}