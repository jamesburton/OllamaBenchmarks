using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EfCore10Demo
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        // EF Core 10 complex type with JSON conversion
        public Address Address { get; set; } = new();
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
    }

    // DTO for ordered data
    public record OrderDto(int OrderId, decimal Total, string CustomerName);

    public class AppDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Complex type mapping with JSON conversion
            modelBuilder.Entity<Customer>()
                .Property(c => c.Address)
                .HasJsonConversion();

            // Example of bulk‑update helpers (no‑load required)
            modelBuilder.Entity<Order>()
                .ToQueryFilter(null); // placeholder to silence warnings
        }
    }

    public static class OrderQueries
    {
        /// <summary>
        /// Uses EF Core 10's .LeftJoin() to join Orders with Customers.
        /// Returns a list of OrderDto where CustomerName is "Unknown" when
        /// no matching Customer exists.
        /// </summary>
        public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
        {
            var query = db.Orders
                .LeftJoin(
                    db.Customers,
                    order => order.CustomerId,
                    customer => customer.Id,
                    (order, customer) => new OrderDto(
                        orderId: order.Id,
                        Total: order.Total,
                        CustomerName: customer == null ? "Unknown" : customer.Name))
                .ToList();

            return query;
        }
    }
}