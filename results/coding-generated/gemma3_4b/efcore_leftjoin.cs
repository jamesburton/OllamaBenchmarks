using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EfCore10Example
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
        public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
        {
            var orders = db.Orders
                .LeftJoin(
                    db.Customers,
                    order => order.CustomerId,
                    customer => customer.Id,
                    (order, customer) => new { order.Id, CustomerName = customer == null ? "Unknown" : customer.Name }
                )
                .Select(x => new OrderDto(x.Id, x.Total, x.CustomerName))
                .ToList();
            return orders;
        }
    }
}