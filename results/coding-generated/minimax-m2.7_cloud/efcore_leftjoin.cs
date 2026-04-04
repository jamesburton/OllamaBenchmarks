using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query; // for LeftJoin

// Entity classes
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

// DbContext
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Optional entity configuration can go here
    }
}

// DTO
public record OrderDto(int OrderId, decimal Total, string CustomerName);

// Static query class
public static class OrderQueries
{
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        // Use EF Core 10's LeftJoin to join Orders with Customers
        var joined = db.Orders.LeftJoin(
            db.Customers,
            order => order.CustomerId,
            customer => customer.Id,
            (order, customer) => new { order, customer });

        // Project to OrderDto, handling missing customers
        var result = joined
            .Select(x => new OrderDto(
                x.order.Id,
                x.order.Total,
                x.customer == null ? "Unknown" : x.customer.Name))
            .ToList();

        return result;
    }
}