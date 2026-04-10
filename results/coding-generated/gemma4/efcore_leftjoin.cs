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

// 4. DTO record
public record OrderDto(int OrderId, decimal Total, string CustomerName);

// 3. DbContext class
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Seed data for testing purposes if needed, though not required by prompt
    }
}

// 5. Static class containing the query method
public static class OrderQueries
{
    /// <summary>
    /// Uses EF Core 10's LeftJoin to join Orders with Customers and projects the result to OrderDto.
    /// </summary>
    /// <param name="db">The application database context.</param>
    /// <returns>A list of OrderDto containing order details and customer name.</returns>
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        // Implementation using LeftJoin as per EF Core 10 reference
        var query = db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new { order, customer } // Intermediate anonymous type to hold both
            )
            .Select(x => new OrderDto(
                x.order.Id,
                x.order.Total,
                // Handle null customer case explicitly as required by the prompt ("Unknown")
                x.customer == null ? "Unknown" : x.customer.Name
            ))
            .ToList();

        return query;
    }
}

// Example usage (optional, for completeness/testing context)
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup in-memory database for demonstration
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureCreated();

            // Seed Data
            context.Customers.AddRange(
                new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" },
                new Customer { Id = 2, Name = "Bob", Email = "bob@example.com" }
            );
            context.Orders.AddRange(
                new Order { Id = 101, CustomerId = 1, Total = 150.00m, OrderDate = DateTime.Now.AddDays(-5) },
                new Order { Id = 102, CustomerId = 2, Total = 25.50m, OrderDate = DateTime.Now.AddDays(-2) },
                // Order with a non-existent customer (to test the "Unknown" case)
                new Order { Id = 103, CustomerId = 99, Total = 50.00m, OrderDate = DateTime.Now.AddDays(-1) }
            );
            await context.SaveChangesAsync();

            // Execute the query
            var results = OrderQueries.GetOrdersWithCustomers(context);

            Console.WriteLine("--- Query Results ---");
            foreach (var dto in results)
            {
                Console.WriteLine($"Order ID: {dto.OrderId}, Total: {dto.Total:C}, Customer: {dto.CustomerName}");
            }
        }
    }
}