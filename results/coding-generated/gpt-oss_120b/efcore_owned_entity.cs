using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Address HomeAddress { get; set; } = new Address();
}

public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; } = null!;

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}