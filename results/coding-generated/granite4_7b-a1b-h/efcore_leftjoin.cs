using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure.Internal;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace YourNamespace
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
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

        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
    }

    public record OrderDto(int OrderId, decimal Total, string CustomerName);

    public static class OrderQueries
    {
        public static async Task<List<OrderDto>> GetOrdersWithCustomers(AppDbContext db)
        {
            var ordersWithCustomers = await db.Orders
                .LeftJoin(
                    db.Customers,
                    order => order.CustomerId,
                    customer => customer.Id,
                    (order, customer) => new { order, customer },
                    (order, customer) => new { order, customer })
                .Where(x => x.customer != null)
                .Select(x => new OrderDto(x.order.Id, x.order.Total, x.customer.Name))
                .ToListAsync();

            var missingCustomers = await db.Orders
                .Where(o => !ordersWithCustomers.Any(o => o.OrderId == o.Id))
                .LeftJoin(
                    db.Customers,
                    order => order.CustomerId,
                    customer => customer.Id,
                    (order, customer) => new { order, customer },
                    (order, customer) => new { order, customer })
                .Select(x => new OrderDto(x.order.Id, x.order.Total, "Unknown"))
                .ToListAsync();

            ordersWithCustomers.AddRange(missingCustomers);
            return ordersWithCustomers;
        }
    }
}