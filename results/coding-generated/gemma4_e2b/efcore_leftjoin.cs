using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Customer entity class
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// 2. Order entity class
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

// 3. AppDbContext class
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure primary keys are set if not implicitly handled by convention
        modelBuilder.Entity<Customer>().HasKey(c => c.Id);
        modelBuilder.Entity<Order>().HasKey(o => o.Id);
    }
}

// 4. OrderDto record
public record OrderDto(int OrderId, decimal Total, string CustomerName);

// 5. Static class OrderQueries
public static class OrderQueries
{
    /// <summary>
    /// Retrieves orders joined with customer names using EF Core 10's LeftJoin.
    /// </summary>
    public static async Task<List<OrderDto>> GetOrdersWithCustomers(AppDbContext db)
    {
        var result = await db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new { order.Id, CustomerName = customer == null ? "Unknown" : customer.Name }
            )
            .Select(joined => new OrderDto(
                joined.order.Id,
                joined.order.Total,
                joined.CustomerName
            ))
            .ToListAsync();

        return result;
    }
}

// Example usage (optional, for completeness, not strictly required by prompt)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup In-Memory Database for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new AppDbContext(options))
        {
            // Seed Data
            context.Customers.AddRange(new List<Customer>
            {
                new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" },
                new Customer { Id = 2, Name = "Bob", Email = "bob@example.com" }
            });
            context.Orders.AddRange(new List<Order>
            {
                new Order { Id = 101, CustomerId = 1, Total = 50.00m, OrderDate = DateTime.Now.AddDays(-5) },
                new Order { Id = 102, CustomerId = 99, Total = 150.50m, OrderDate = DateTime.Now.AddDays(-3) } // Order with unknown customer
            });
            await context.SaveChangesAsync();

            // Execute Query
            var dtos = await OrderQueries.GetOrdersWithCustomers(context);

            // Output Results
            Console.WriteLine("--- Orders with Customer Names ---");
            foreach (var dto in dtos)
            {
                Console.WriteLine($"Order ID: {dto.OrderId}, Total: {dto.Total:C}, Customer: {dto.CustomerName}");
            }
        }
    }
}
*/