using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

[ComplexType]
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Address Address { get; set; }
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
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Example of HasJsonConversion - not strictly required for this problem
        // modelBuilder.Entity<Customer>()
        //     .Property(c => c.Address)
        //     .HasJsonConversion();
    }
}

public record OrderDto(int OrderId, decimal Total, string CustomerName);

public static class OrderQueries
{
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        return db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new { order, customer })
            .Select(joined => new OrderDto(
                joined.order.Id,
                joined.order.Total,
                (joined.customer == null) ? "Unknown" : joined.customer.Name
            ))
            .ToList();
    }
}