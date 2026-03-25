using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
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
                    (order, customer) => new OrderDto(order.Id, order.Total, customer == null ? "Unknown" : customer.Name)
                )
                .ToList();
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            using (var db = new AppDbContext(options))
            {
                // Seed data
                db.Customers.AddRange(
                    new Customer { Id = 1, Name = "John Doe", Email = "john.doe@example.com" },
                    new Customer { Id = 2, Name = "Jane Doe", Email = "jane.doe@example.com" }
                );
                db.Orders.AddRange(
                    new Order { Id = 1, CustomerId = 1, Total = 100m, OrderDate = DateTime.Now },
                    new Order { Id = 2, CustomerId = 2, Total = 200m, OrderDate = DateTime.Now }
                );
                await db.SaveChangesAsync();

                var ordersWithCustomers = OrderQueries.GetOrdersWithCustomers(db);
                foreach (var order in ordersWithCustomers)
                {
                    Console.WriteLine($"Order ID: {order.OrderId}, Total: {order.Total}, Customer: {order.CustomerName}");
                }
            }
        }
    }
}