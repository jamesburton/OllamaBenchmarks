using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

// 1. Address class
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
    public Address HomeAddress { get; set; } = new Address(); // Initialize to a new Address instance
}

// 3. CustomerDbContext class
public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Customer to own the Address property (OwnsOne)
        modelBuilder.Entity<Customer>()
            .OwnsOne(c => c.HomeAddress);
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

// Example usage context (optional, for completeness, not strictly required by prompt)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup DbContext (Example using in-memory for demonstration)
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new CustomerDbContext(options))
        {
            // Ensure database is created (for in-memory)
            context.Database.EnsureCreated();

            var repository = new CustomerRepository(context);

            // Create a new customer
            var newCustomer = new Customer
            {
                Name = "Alice Smith",
                HomeAddress = new Address { Street = "123 Main St", City = "London", Zip = "SW1A 0AA" }
            };
            await repository.CreateAsync(newCustomer);
            Console.WriteLine($"Created customer with ID: {newCustomer.Id}");

            // Get the customer
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