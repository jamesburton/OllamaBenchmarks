using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EfCore10QuickReference
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
    }

    public record OrderDto(int OrderId, decimal Total, string CustomerName);

    public static class OrderQueries
    {
        public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
        {
            return db.Orders
                .LeftJoin(
                    db.Customers,
                    order => order.CustomerId,
                    customer => customer.Id,
                    (order, customer) => new OrderDto(
                        order.Id,
                        order.Total,
                        customer == null ? "Unknown" : customer.Name
                    )
                )
                .ToList();
        }
    }
}