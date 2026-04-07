using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

public class Address
{
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Zip { get; set; } = default!;
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public Address HomeAddress { get; set; } = new Address();
}

public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; } = default!;

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>()
            .OwnsOne(c => c.HomeAddress);
    }
}

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