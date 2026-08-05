using Microsoft.EntityFrameworkCore;

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Zip { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HomeAddress { get; set; } = new Address(); // Initialized to avoid null
}

public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress);
    }
}

public class CustomerRepository
{
    private readonly CustomerDbContext context;

    public CustomerRepository(CustomerDbContext context)
    {
        this.context = context;
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await context.Customers.FindAsync(id);
    }
}