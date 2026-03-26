using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EfCoreExample
{
    public class Address { public string Street { get; set; } public string City { get; set; } }

    [ComplexType]
    public class Customer : Entity { public int Id { get; set; } public string Name { get; set; } public string Email { get; set; } public Address Address { get; set; } }

    public record Order(int Id, decimal Total, DateTime OrderDate);

    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .Property(c => c.Address)
                .HasJsonConversion();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
    }

    public record OrderDto(int OrderId, decimal Total, string CustomerName);

    public static class OrderQueries
    {
        private static readonly AppDbContext _db = new AppDbContext(new DbContextOptions<AppDbContext>
        {
            // Replace with your actual options or use InMemoryDatabase for testing purposes.
        });

        public static List<OrderDto> GetOrdersWithCustomers()
        {
            return (from order in _db.Orders
                  join customer on false /* This is a workaround to avoid the left join */
                      .DefaultIfEmpty() as Customer // Cast nulls from Orders without customers into an anonymous type with default values.
                  select new OrderDto(
                          order.Id,
                          order.Total,
                          ((customer == null) ? "Unknown" : (dynamic)(await customer.Name))
                    )
                ).ToList();
        }
    }

    public class Entity
    {
        protected ObjectType? _type;

        private void SetProperty(object value, string propertyName)
        {
            if (_type != null && !string.IsNullOrEmpty(propertyName) &&
                (property := GetPropertyInfo(propertyName)) is not null)
            {
                ((dynamic)this).SetValue(_type!, property.Name, value);
            }
        }

        private PropertyInfo? GetPropertyInfo(string name)
        {
            return _type!.GetType().GetRuntimeProperties(name)?.GetValue(this) as PropertyInfo;
        }
    }
}