using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Core.Entities
{
    [ComplexType]
    public class Address
    {
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;

        // Optional JSON property for demonstration of EF10 features mentioned in reference
        [ComplexType] 
        public Address? Address { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }

        // Navigation property for clarity, though not strictly needed for the LeftJoin query shown below
        public virtual Customer? Customer { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Customer> Customers => Set<Customer>();

        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Optional: Configure JSON conversion if Address is used as a complex type with JSON column
            /*
            builder.Entity<Customer>()
                .Property(c => c.Address)
                .HasJsonConversion(); 
             */

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }

    // DTO for the query result
    public record OrderDto(
        int OrderId, 
        decimal Total, 
        string CustomerName
    );

    public static class OrderQueries
    {
        /// <summary>
        /// Retrieves all orders joined with customers using EF Core 10's LeftJoin.
        /// If no customer matches an order, the CustomerName is set to "Unknown".
        /// </summary>
        public static async Task<List<OrderDto>> GetOrdersWithCustomers(AppDbContext db)
        {
            var query = await db.Orders
                .LeftJoin(
                    db.Customers,
                    order => order.CustomerId,
                    customer => customer.Id,
                    (order, customer) => new OrderDto(
                        orderId: order.Id, 
                        Total: order.Total, 
                        CustomerName: customer == null ? "Unknown" : customer.Name
                    )
                ).ToListAsync();

            return query;
        }
    }
}