using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
}

public class OrderQueries
{
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        var orders = db.Orders
            .Where(o => o.Status == "Pending")
            .Select(o => new OrderDto(o.Id, o.Total, o.CustomerName))
            .ToList();

        return orders;
    }
}

public class OrderDto
{
    public int OrderId { get; set; }
    public decimal Total { get; set; }
    public string CustomerName { get; set; }
}

public class OrderQueries
{
    public static void Main(string[] args)
    {
        var appDbContextOptions = new DbContextOptions<AppDbContext>
        {
            Name = "TestDb",
            ConnectionStrings = new string[] { "Server=local;Database=TestDb;IntegratedSecurity=True" }
        };

        var appDbContext = new AppDbContext(appDbContextOptions);

        var orders = OrderQueries.GetOrdersWithCustomers(appDbContext);

        foreach (var order in orders)
        {
            Console.WriteLine($"Order Id: {order.OrderId}, Total: {order.Total}, Customer Name: {order.CustomerName}");
        }
    }
}