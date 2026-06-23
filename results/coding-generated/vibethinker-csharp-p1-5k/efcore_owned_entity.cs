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

    public Customer() => Id = Guid.NewGuid().ToString(), Name = "Default", HomeAddress = new Address();
}

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) => base options;

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress);
    }
}

public class CustomerRepository
{
    private readonly CustomerDbContext _context;
    private readonly IAsyncTaskHandler _handler;

    public CustomerRepository(CustomerDbContext context, IAsyncTaskHandler handler) => 
        _context = context,
        _handler = handler;

    public Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customer.Add(customer);
        await _handler.ExecuteAsync(_context.SaveChangesAsync());
        return customer;
    }

    public Task<Customer?> GetCustomerAsync(int id)
    {
        var customer = await _context.Customer
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (customer != null)
        {
            await _handler.ExecuteAsync(_context.SaveChangesAsync());
            return customer;
        }
        else
        {
            return null;
        }
    }
}