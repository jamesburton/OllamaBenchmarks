using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// Entity classes
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public Customer Customer { get; set; }
}

// DTO
public record OrderDto(int OrderId, decimal Total, string CustomerName);

// DbContext
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Left join example (for EF Core 10+)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId);

        // JSON property example (if needed)
        // modelBuilder.Entity<Customer>()
        //     .Property(c => c.Address)
        //     .HasConversion<Address, Dictionary<string, object>>();

        // Complex type (if needed)
        // modelBuilder.Entity<Customer>()
        //     .OwnsOne(c => c.Address);
    }
}

// Query logic
public static class OrderQueries
{
    public static List<OrderDto> GetOrdersWithCustomers(AppDbContext db)
    {
        var query = db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new
                {
                    Order = order,
                    CustomerName = customer?.Name ?? "Unknown"
                })
            .Select(projection => new OrderDto(
                projection.Order.Id,
                projection.Order.Total,
                projection.CustomerName));

        return query.ToList();
    }
}