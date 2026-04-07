using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EfCore10Demo
{
    // -----------------------------------------------------------------
    // Entity classes
    // -----------------------------------------------------------------
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
    }

    // -----------------------------------------------------------------
    // DbContext
    // -----------------------------------------------------------------
    public class AppDbContext : DbContext
    {
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Optional: configure model (not required for this example)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    // -----------------------------------------------------------------
    // DTO
    // -----------------------------------------------------------------
    public record OrderDto(int OrderId, decimal Total, string CustomerName);

    // -----------------------------------------------------------------
    // Query helper
    // -----------------------------------------------------------------
    public static class OrderQueries
    {
        /// <summary>
        /// Retrieves all orders together with the associated customer name.
        /// If an order has no matching customer, the name is reported as "Unknown".
        /// </summary>
        public static async Task<List<OrderDto>> GetOrdersWithCustomersAsync(AppDbContext db)
        {
            var query = db.Orders.LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new OrderDto(
                    OrderId: order.Id,
                    Total: order.Total,
                    CustomerName: customer == null ? "Unknown" : customer.Name));

            return await query.ToListAsync();
        }

        // Synchronous version (if preferred)
        public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
        {
            var query = db.Orders.LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new OrderDto(
                    OrderId: order.Id,
                    Total: order.Total,
                    CustomerName: customer == null ? "Unknown" : customer.Name));

            return query.ToList();
        }
    }
}