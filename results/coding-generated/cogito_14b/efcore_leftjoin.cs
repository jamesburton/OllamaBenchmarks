using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public class OrderDto
    {
        public int OrderId { get; init; }
        public decimal Total { get; init; }
        public string CustomerName { get; init; }

        public OrderDto(int orderId, decimal total, string customerName)
        {
            OrderId = orderId;
            Total = total;
            CustomerName = customerName;
        }
    }

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
    }

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