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
        // Seed data for demonstration purposes (optional, but helpful for testing)
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" },
            new Customer { Id = 2, Name = "Bob", Email = "bob@example.com" }
        );
        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 101, CustomerId = 1, Total = 50.00m, OrderDate = DateTime.Now.AddDays(-5) },
            new Order { Id = 102, CustomerId = 99, Total = 150.50m, OrderDate = DateTime.Now.AddDays(-3) } // Order with non-existent customer
        );
    }
}

// 4. OrderDto record
public record OrderDto(int OrderId, decimal Total, string CustomerName);

// 5. Static class with the query method
public static class OrderQueries
{
    /// <summary>
    /// Retrieves orders joined with customer names using EF Core 10's LeftJoin.
    /// </summary>
    /// <param name="db">The DbContext to query against.</param>
    /// <returns>A list of OrderDto containing order details and customer names.</returns>
    public static async Task<List<OrderDto>> GetOrdersWithCustomers(AppDbContext db)
    {
        var results = await db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new { order.Id, CustomerName = customer == null ? "Unknown" : customer.Name }
            )
            .Select(joined => new OrderDto(
                OrderId: joined.order.Id,
                Total: joined.order.Total,
                CustomerName: joined.CustomerName
            ))
            .ToListAsync();

        return results;
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
            // Ensure database is created and seeded (if not using seed data in OnModelCreating)
            // context.Database.EnsureCreated(); 

            Console.WriteLine("Executing LeftJoin query...");
            var orderDtos = await OrderQueries.GetOrdersWithCustomers(context);

            Console.WriteLine("\n--- Results ---");
            foreach (var dto in orderDtos)
            {
                Console.WriteLine($"Order ID: {dto.OrderId}, Total: {dto.Total:C}, Customer: {dto.CustomerName}");
            }
        }
    }
}
*/