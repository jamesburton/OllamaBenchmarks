using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

// 1. Address Class (Owned Type)
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
    // Initialize HomeAddress to avoid null reference issues
    public Address HomeAddress { get; set; } = new Address();
}

// 3. DbContext
public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure HomeAddress as an owned type
        modelBuilder.Entity<Customer>()
            .OwnsOne(c => c.HomeAddress);
    }
}

// 4. Repository Class
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