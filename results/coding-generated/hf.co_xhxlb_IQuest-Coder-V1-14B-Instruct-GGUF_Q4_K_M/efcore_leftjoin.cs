using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFCommon
{
    // Customer entity class
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    // Order entity class
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public Customer Customer { get; set; }
    }

    // AppDbContext class
    public class AppDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }

    // OrderDto record
    public record OrderDto(int OrderId, decimal Total, string CustomerName);

    // OrderQueries static class
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