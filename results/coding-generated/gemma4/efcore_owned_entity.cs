using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

// 1. Address class (Owned Type)
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Zip { get; set; }
}

// 2. Customer class
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    // Must be initialized to avoid null reference issues when EF Core hydrates
    public Address HomeAddress { get; set; } = new Address();
}

// 3. CustomerDbContext class
public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Address as an owned type for Customer
        modelBuilder.Entity<Customer>()
            .OwnsOne(c => c.HomeAddress, addressBuilder =>
            {
                // Optionally rename the JSON column if needed, but for simplicity, we let EF handle it.
                addressBuilder.ToJson(); 
            });
    }
}

// 4. CustomerRepository class
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