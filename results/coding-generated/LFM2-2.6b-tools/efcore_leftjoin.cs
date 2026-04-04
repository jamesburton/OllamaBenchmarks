using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public int OrderId { get; set; }
    public decimal Total { get; set; }
    public string CustomerName { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .Property(c => c.Address)
            .HasJsonConversion();
    }
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
                (order, customer) => new { order.Id, order.Total, CustomerName = customer?.Name ?? "Unknown" })
            .Select(o => new OrderDto
            {
                OrderId = o.Id,
                Total = o.Total,
                CustomerName = o.CustomerName
            })
            .ToList();
    }
}

// Example usage (not part of the required file):
// var options = new DbContextOptionsBuilder<AppDbContext>()
//     .UseInMemoryDatabase(databaseName: "TestDb")
//     .Options;
// using (var context = new AppDbContext(options))
// {
//     var orders = OrderQueries.GetOrdersWithCustomers(context);
// }