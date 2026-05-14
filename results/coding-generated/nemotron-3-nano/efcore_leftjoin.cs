using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace EfCore10Demo;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

public record OrderDto(int OrderId, decimal Total, string CustomerName);

public class AppDbContext : DbContext
{
    public DbSet<Order>? Orders { get; set; }
    public DbSet<Customer>? Customers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Example of complex type with JSON column
        modelBuilder.Entity<Customer>()
            .Property(c => c.Email)
            .HasConversion(v => v, v => v); // placeholder; adjust as needed

        // Example of bulk update on JSON nested property (if needed)
        // modelBuilder.Entity<Customer>()
        //     .Property(c => c.Address)
        //     .HasJsonConversion();

        // LeftJoin usage example (in queries, not here)
        modelBuilder.Entity<Order>()
            .ToTable("Orders"); // optional configuration
    }
}

public static class OrderQueries
{
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        var query = db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new { Order = order, Customer = customer }
            )
            .Select(x => x.Order) // keep only Order for projection
            .ToList(); // materialize to List<object>

        // The above query returns a list of anonymous objects; we need a
        // stronger typing to get OrderDto. Instead we can project directly:
        var result = db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new
                {
                    OrderId = order.Id,
                    Total = order.Total,
                    CustomerName = customer == null ? "Unknown" : customer.Name
                }
            )
            .Select(x => new OrderDto(x.OrderId, x.Total, x.CustomerName))
            .ToList();

        return result;
    }
}