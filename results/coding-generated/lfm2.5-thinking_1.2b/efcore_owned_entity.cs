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
    public Address HomeAddress { get; set; }
}

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }
    public DbSet<Customer> Customers { get; set; }
    public override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .Property(c => c.HomeAddress)
            .HasJsonConversion();
    }
}

public class CustomerRepository
{
    private readonly CustomerDbContext _context;
    public CustomerRepository(DbContextOptions<CustomerDbContext> options) : _context(options) { }
    public async Task<Customer> CreateAsync(Customer customer)
    {
        var entity = new Customer { Id = customer.Id, Name = customer.Name, HomeAddress = customer.HomeAddress };
        await _context.Customers.AddAsync(entity);
        return entity;
    }
    public async Task<Customer?> GetByIdAsync(int id)
    {
        return _context.Customers.Find(id);
    }
}