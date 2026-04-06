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
            public Address HomeAddress { get; set; } = new Address();
        }

public class CustomerDbContext : DbContext
        {
            public DbSet<Customer> Customers { get; set; }

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

            public Task<Customer> CreateAsync(Customer customer)
            {
                _context.Customers.Add(customer);
                return _context.SaveChangesAsync(); // Wait, needs to return the entity.
                // Correction: Add, SaveChanges, then return the entity.
            }

            public Task<Customer?> GetByIdAsync(int id)
            {
                return _context.Customers.FindAsync(id);
            }
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