using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// 1. Address Class (EF Core Owned Type)
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Zip { get; set; }
}

// 2. Customer Class
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HomeAddress { get; set; } = new Address(); // Initialize to avoid null issues
}

// 3. CustomerDbContext Class
public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Customer to own the Address entity
        modelBuilder.Entity<Customer>()
            .OwnsOne(c => c.HomeAddress);
    }
}

// 4. CustomerRepository Class
public class CustomerRepository
{
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }
}

// Example Usage (Optional, for demonstration purposes, not required by prompt)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup In-Memory Database Context
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(databaseName: "TestCustomerDb")
            .Options;

        using (var context = new CustomerDbContext(options))
        {
            context.Database.EnsureCreated();

            // Setup Repository
            var repository = new CustomerRepository(context);

            // Create Customer
            var newCustomer = new Customer
            {
                Name = "Alice Smith",
                HomeAddress = new Address { Street = "123 Main St", City = "London", Zip = "SW1A 0AA" }
            };
            await repository.CreateAsync(newCustomer);
            Console.WriteLine($"Created Customer with ID: {newCustomer.Id}");

            // Get Customer
            var retrievedCustomer = await repository.GetByIdAsync(newCustomer.Id);
            if (retrievedCustomer != null)
            {
                Console.WriteLine($"Retrieved Customer: {retrievedCustomer.Name}");
                Console.WriteLine($"Address: {retrievedCustomer.HomeAddress.Street}, {retrievedCustomer.HomeAddress.City}");
            }
        }
    }
}
*/