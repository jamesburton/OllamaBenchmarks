using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EfCore10Demo
{
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

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
    }

    public record OrderDto(int OrderId, decimal Total, string CustomerName);

    public static class OrderQueries
    {
        public static async Task<List<OrderDto>> GetOrdersWithCustomers(AppDbContext db)
        {
            return await db.Orders
                .LeftJoin(db.Customers,
                    o => o.CustomerId,
                    c => c.Id,
                    (o, c) => new OrderDto
                    {
                        OrderId = o.Id,
                        Total = o.Total,
                        CustomerName = c == null ? "Unknown" : c.Name
                    })
                .ToListAsync();
        }
    }
}