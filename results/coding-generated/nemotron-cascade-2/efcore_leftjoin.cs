using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EfCore10Demo
{
    // 1. Customer entity
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    // 2. Order entity
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
    }

    // 3. DbContext
    public class AppDbContext : DbContext
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Customer> Customers => Set<Customer>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // No further configuration needed for this example.
    }

    // 4. DTO record
    public record OrderDto(int OrderId, decimal Total, string CustomerName);

    // 5. Query helper
    public static class OrderQueries
    {
        public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
        {
            var query = db.Orders.LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new
                {
                    OrderId = order.Id,
                    Total = order.Total,
                    CustomerName = customer == null ? "Unknown" : customer.Name
                });

            return query
                .Select(x => new OrderDto(x.OrderId, x.Total, x.CustomerName))
                .ToList();
        }
    }
}