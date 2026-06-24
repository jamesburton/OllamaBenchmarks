using Microsoft.EntityFrameworkCore;

public class Address
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string Zip { get; set; } = "";
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Address HomeAddress { get; set; } = new();
}

public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
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

    public Task<Customer> CreateAsync(Customer customer)
    {
        return _context.Add(customer).ThenSaveAsync();
    }

    public Task<Customer?> GetByIdAsync(int id)
    {
        return _context.Customers.FindAsync(id);
    }
}