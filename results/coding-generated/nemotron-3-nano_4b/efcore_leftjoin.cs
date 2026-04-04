using System;
using System.Collections.Generic;
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
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();
}

public record OrderDto(int OrderId, decimal Total, string CustomerName);

static class OrderQueries
{
    public static List<OrderDto> GetOrdersWithCustomers(DbContext db)
    {
        return db.Orders
            .LeftJoin(
                db.Customers,
                o => o.CustomerId,
                c => c.Id,
                (o, c) => new OrderDto(o.Id, o.Total, c?.Name ?? "Unknown"))
            .ToList();
    }
}