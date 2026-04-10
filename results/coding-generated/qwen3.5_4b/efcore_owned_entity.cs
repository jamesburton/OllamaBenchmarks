using Microsoft.EntityFrameworkCore;

    public class Address { ... }
    public class Customer { ... }
    public class CustomerDbContext : DbContext { ... }
    public class CustomerRepository { ... }

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

        public Customer()
        {
            HomeAddress = new Address();
        }
    }

    public class CustomerDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }

        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
        {
        }

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

        public Task<Customer> CreateAsync(Customer customer)
        {
            return _context.Customers.AddAsync(customer).ThenResult(_context.SaveChangesAsync());
        }

        public Task<Customer?> GetByIdAsync(int id)
        {
            return _context.Customers.FindAsync(id);
        }
    }

public Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        return _context.SaveChangesAsync().AsTask(); // Wait, SaveChangesAsync returns Task<int>.
    }

public async Task<Customer> CreateAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

public Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        return _context.SaveChangesAsync().AsTask(); // No, SaveChangesAsync returns Task<int>.
    }

public async Task<Customer> CreateAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

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

        public Customer()
        {
            HomeAddress = new Address();
        }
    }

    public class CustomerDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }

        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
        {
        }

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
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public Task<Customer?> GetByIdAsync(int id)
        {
            return _context.Customers.FindAsync(id);
        }
    }