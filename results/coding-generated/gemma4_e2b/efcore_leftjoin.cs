using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Customer Entity Class
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// 2. Order Entity Class
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

// 4. Order DTO Record
public record OrderDto(int OrderId, decimal Total, string CustomerName);

// 3. AppDbContext Class
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Optional: Configure relationships if needed
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany() // Assuming Customer doesn't reference Order directly for simplicity
            .HasForeignKey(o => o.CustomerId);
    }
}

// 5. Static Class for Queries
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
                (order, customer) => new
                {
                    OrderId = order.Id,
                    Total = order.Total,
                    CustomerName = customer == null ? "Unknown" : customer.Name
                }
            )
            .Select(joined => new OrderDto(
                joined.OrderId,
                joined.Total,
                joined.CustomerName
            )
            )
            .ToListAsync();

        return result;
    }
}

// Example Usage (Demonstration purposes only, requires setup to run)
public class Program
{
    public static async Task Main(string[] args)
    {
        // This section is illustrative and requires a configured DbContext setup to run.
        // var options = new DbContextOptionsBuilder<AppDbContext>()
        //     .UseInMemoryDatabase(databaseName: "TestDb")
        //     .Options;

        // var context = new AppDbContext(options);
        // await context.Database.EnsureCreatedAsync();

        // // Seed data (omitted for brevity)

        // // var ordersWithCustomers = await OrderQueries.GetOrdersWithCustomers(context);
        // // Console.WriteLine("Orders with Customer Names:");
        // // foreach (var dto in ordersWithCustomers)
        // // {
        // //     Console.WriteLine($"Order ID: {dto.OrderId}, Total: {dto.Total}, Customer: {dto.CustomerName}");
        // // }
    }
}