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

// 3. AppDbContext class
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}

public static class OrderQueries
{
    /// <summary>
    /// Uses EF Core 10's LeftJoin to retrieve orders and their associated customer names.
    /// </summary>
    /// <param name="db">The application database context.</param>
    /// <returns>A list of OrderDto containing order details and customer name.</returns>
    public static async Task<List<OrderDto>> GetOrdersWithCustomers(AppDbContext db)
    {
        // Implementation using the LeftJoin pattern from EF Core 10
        var query = db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new { order, customer } // Projecting the joined objects
            );

        // Now we project the result into the desired DTO structure
        var results = await query
            .Select(joined => new OrderDto(
                joined.order.Id,
                joined.order.Total,
                // Handle the null case for the customer name
                joined.customer == null ? "Unknown" : joined.customer.Name
            ))
            .ToListAsync();

        return results;
    }
}

// Example usage (optional, for testing purposes)
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
            // Seed data
            context.Customers.AddRange(
                new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" },
                new Customer { Id = 2, Name = "Bob", Email = "bob@example.com" }
            );
            context.Orders.AddRange(
                new Order { Id = 101, CustomerId = 1, Total = 50.00m, OrderDate = DateTime.Now.AddDays(-1) },
                new Order { Id = 102, CustomerId = 2, Total = 150.50m, OrderDate = DateTime.Now.AddDays(-2) },
                // Order 103 has a CustomerId (3) that does not exist, testing the Left Join null case
                new Order { Id = 103, CustomerId = 3, Total = 25.00m, OrderDate = DateTime.Now }
            );
            await context.SaveChangesAsync();

            // Execute the query
            var results = await OrderQueries.GetOrdersWithCustomers(context);

            // Output results
            Console.WriteLine("--- Orders with Customers (Left Join Test) ---");
            foreach (var dto in results)
            {
                Console.WriteLine($"Order ID: {dto.OrderId}, Total: {dto.Total:C}, Customer: {dto.CustomerName}");
            }
            // Expected output: 
            // 101 -> Alice
            // 102 -> Bob
            // 103 -> Unknown (because CustomerId 3 does not exist)
        }
    }
}