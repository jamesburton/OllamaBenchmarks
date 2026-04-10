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

// 4. OrderDto record
public record OrderDto(int OrderId, decimal Total, string CustomerName);

// 3. AppDbContext class
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}

// 5. Static class containing the query logic
public static class OrderQueries
{
    /// <summary>
    /// Uses EF Core 10's LeftJoin to retrieve orders and their associated customer names.
    /// </summary>
    /// <param name="db">The application database context.</param>
    /// <returns>A list of OrderDto containing order details and customer name.</returns>
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        // Implementation using the LeftJoin pattern
        var query = db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new 
                { 
                    order.Id, 
                    Total = order.Total, 
                    OrderDate = order.OrderDate,
                    // Handle null customer name case
                    CustomerName = customer == null ? "Unknown" : customer.Name 
                }
            );

        // Project the anonymous type result into the desired DTO
        return query.Select(x => new OrderDto(
            x.Id, 
            x.Total, 
            x.CustomerName
        )).ToList();
    }
}

// Note: To run this code, you would need to set up a console application 
// and configure the DbContextOptions (e.g., using UseInMemoryDatabase).