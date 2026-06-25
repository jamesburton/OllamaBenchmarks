using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }

    [InverseProperty("Orders")]
    public virtual Customer Customer { get; set; }
}

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Add connection string here if needed.
        options.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=TestDb");
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}

public class OrderDto
{
    public int OrderId { get; set; }
    public decimal Total { get; set; }
    public string CustomerName { get; set; }

    public OrderDto(int orderId, decimal total, string customerName)
    {
        OrderId = orderId;
        Total = total;
        CustomerName = customerName ?? "Unknown";
    }
}

public static class OrderQueries
{
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        var query = db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new { order.Id, CustomerName = customer == null ? "N/A" : customer.Name }
            );

        return query.Select(o => new OrderDto(o.OrderId, o.Total, o.CustomerName)).ToList();
    }
}