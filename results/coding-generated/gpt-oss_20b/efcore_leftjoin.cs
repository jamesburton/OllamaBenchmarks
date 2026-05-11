using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace MyApp
{
    // 1. Customer entity
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
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
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
    }

    // 4. DTO
    public record OrderDto(int OrderId, decimal Total, string CustomerName);

    // 5. Query helper
    public static class OrderQueries
    {
        public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
        {
            var query = db.Orders
                .LeftJoin(
                    db.Customers,
                    o => o.CustomerId,
                    c => c.Id,
                    (o, c) => new OrderDto(
                        o.Id,
                        o.Total,
                        c == null ? "Unknown" : c.Name
                    )
                );

            return query.ToList();
        }
    }
}