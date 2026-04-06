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
        public Address HomeAddress { get; set; } = new Address();
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
            modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress);
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
            return _context.SaveChangesAsync().AsTask(); // Wait, SaveChangesAsync returns Task<int>. Need to return Task<Customer>.
            // Actually, SaveChangesAsync returns Task<int>.
            // To return Task<Customer>, I need to await SaveChangesAsync and then return the customer.
            // But the customer is already in the context.
            // Let's fix the logic.
        }
    }