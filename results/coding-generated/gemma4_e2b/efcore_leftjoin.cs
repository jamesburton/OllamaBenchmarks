using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Customer Entity
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// 2. Order Entity
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

// 4. Order DTO
public record OrderDto(int OrderId, decimal Total, string CustomerName);

// 3. AppDbContext
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure relationships are defined if necessary (optional for this specific query)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany() // Assuming Customer doesn't reference Order directly for simplicity
            .HasForeignKey(o => o.CustomerId);
    }
}

// 5. Static Class with Query Logic
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
                (order, customer) => new { order, customer } // Define the join result structure
            )
            .Select(joined => new OrderDto(
                OrderId: joined.order.Id,
                Total: joined.order.Total,
                // Handle the null case for the left join
                CustomerName: joined.customer != null ? joined.customer.Name : "Unknown"
            ))
            .ToListAsync();

        return result;
    }
}

// Example Usage (Demonstration purposes only, requires setup of a DbContext)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup In-Memory Database for demonstration
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
                new Order { CustomerId = 1, Total = 100.50m, OrderDate = DateTime.Now.AddDays(-5) },
                new Order { CustomerId = 99, Total = 50.00m, OrderDate = DateTime.Now.AddDays(-3) } // Order with unknown customer
            });

            await context.SaveChangesAsync();

            // Execute the query
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