using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Address? Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public Customer Customer { get; set; }
}

public class OrderDto
{
    public int OrderId { get; set; }
    public decimal Total { get; set; }
    public string CustomerName { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .Property(c => c.Address)
            .HasJsonConversion();
    }
}

public static class OrderQueries
{
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        return db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new { order.Id, order.Total, order.OrderDate, CustomerName = customer?.Name ?? "Unknown" })
            .Select(o => new OrderDto
            {
                OrderId = o.Id,
                Total = o.Total,
                CustomerName = o.CustomerName
            })
            .ToList();
    }
}

// Example usage (not part of the class file but for completeness):
/*
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: "TestDb")
    .Options;

using var context = new AppDbContext(options);

// Seed data
context.Customers.Add(new Customer { Id = 1, Name = "Alice", Email = "alice@example.com", Address = new Address { City = "London" } });
context.Customers.Add(new Customer { Id = 2, Name = "Bob", Email = "bob@example.com" });
context.Orders.Add(new Order { Id = 1, CustomerId = 1, Total = 100m, OrderDate = DateTime.UtcNow });
context.Orders.Add(new Order { Id = 2, CustomerId = 3, Total = 200m, OrderDate = DateTime.UtcNow }); // No customer
context.SaveChanges();

var orders = OrderQueries.GetOrdersWithCustomers(context);
// orders will contain 2 entries:
// - OrderId: 1, Total: 100, CustomerName: Alice
// - OrderId: 2, Total: 200, CustomerName: Unknown
*/