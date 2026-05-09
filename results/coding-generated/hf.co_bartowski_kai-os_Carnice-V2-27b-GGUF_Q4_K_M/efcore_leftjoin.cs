using System;
using System.Collections.Generic;
using System.Linq;
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
        var query = db.Orders
            .LeftJoin(
                db.Customers,
                order => order.CustomerId,
                customer => customer.Id,
                (order, customer) => new OrderDto(
                    order.Id,
                    order.Total,
                    customer == null ? "Unknown" : customer.Name
                )
            );

        return query.ToList();
    }
}

var query = db.Orders
    .LeftJoin(
        db.Customers,
        order => order.CustomerId,
        customer => customer.Id,
        (order, customer) => new { order.Id, CustomerName = customer == null ? "N/A" : customer.Name }
    );

[ComplexType]
public class Address { public string Street { get; set; } public string City { get; set; } }

// In OnModelCreating:
builder.Entity<Customer>()
    .Property(c => c.Address)
    .HasJsonConversion();

await db.Orders
    .Where(o => o.Status == "Pending")
    .ExecuteUpdateAsync(s => s
        .SetProperty(o => o.Status, "Cancelled")
        .SetProperty(o => o.UpdatedAt, DateTime.UtcNow));

c
</think>

</think>

</think>

</think>

</think>

</think>

</think>

</think>

</think>

</think>

</think>

</think>