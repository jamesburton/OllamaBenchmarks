using Microsoft.EntityFrameworkCore;

public class Address {
    public string Street { get; set; }
    public string City { get; set; }
    public string Zip { get; set; }
}

public class Customer {
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HomeAddress { get; set; }

    public Customer() {
        HomeAddress = new Address();
    }
}

public class CustomerDbContext : DbContext {
    public DbSet<Customer> Customers { get; set; }

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Customer>()
            .OwnsOne(c => c.HomeAddress);
    }
}

public class CustomerRepository {
    private readonly CustomerDbContext _ctx;

    public CustomerRepository(CustomerDbContext context) {
        _ctx = context;
    }

    public async Task<Customer> CreateAsync(Customer customer) {
        if (customer == null) throw new ArgumentNullException(nameof(customer));

        _ctx.Customers.Add(customer);
        await _ctx.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> GetByIdAsync(int id) {
        return await _ctx.Customers.FirstOrDefaultAsync(c => c.Id == id);
    }
}