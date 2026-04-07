protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       modelBuilder.Entity<Customer>(entity =>
       {
           entity.OwnsProperty(c => c.HomeAddress, a => a.ToEntity());
           // But wait, the requirement says: modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress)
           // Actually, the method is `OwnsOne` and it takes an expression for the property. So we do:
           // modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress);
           // However, the `OwnsOne` method is called on `Entity` builder for `Customer`. So we can write:
           // modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress);
           // But note: the `OwnsOne` method returns `OwnedNavigationPropertyBuilder<Customer, Address>` which we can then configure further? But the requirement just says to configure it.
           // Actually, we don't need to configure anything else? The requirement only says to call `OwnsOne` for the property.
       });
   }

public async Task<Customer?> GetByIdAsync(int id)
   {
       return await context.Customers.FirstOrDefaultAsync(c => c.Id == id);
   }

protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress);
   }

public class Customer
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public Address HomeAddress { get; set; } = new Address();
   }

public class Customer
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public Address HomeAddress { get; set; } = new Address();
   }

public class Address
   {
       public string Street { get; set; }
       public string City { get; set; }
       public string Zip { get; set; }
   }

protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress);
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
           // But wait, the requirement says: Task<Customer> CreateAsync(Customer customer)
           // We are to add the customer, save changes, and return the entity.
           // However, note: the return type is Task<Customer>, not Task<Customer?>. So we assume the customer is added successfully.
           // But what if the customer already exists? We are adding a new one. So we do:
           _context.Customers.Add(customer);
           await _context.SaveChangesAsync();
           return customer;
       }

       public async Task<Customer?> GetByIdAsync(int id)
       {
           // We return the customer with the given Id, or null if not found.
           // We can use FirstOrDefaultAsync or SingleOrDefaultAsync. Since Id is unique, we can use SingleOrDefaultAsync.
           // But note: SingleOrDefaultAsync throws if more than one? Actually, it throws if more than one. But we have a unique Id, so it's safe.
           // Alternatively, we can use FirstOrDefaultAsync which returns the first or null.
           // Let's use FirstOrDefaultAsync for simplicity.
           return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
       }
   }

public async Task<Customer> CreateAsync(Customer customer)
   {
       _context.Customers.Add(customer);
       await _context.SaveChangesAsync();
       return customer;
   }

var addedCustomer = await _context.Customers.AddAsync(customer);
   await _context.SaveChangesAsync();
   return addedCustomer;

_context.Customers.Add(customer);
   await _context.SaveChangesAsync();
   return customer;

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

       public async Task<Customer> CreateAsync(Customer customer)
       {
           _context.Customers.Add(customer);
           await _context.SaveChangesAsync();
           return customer;
       }

       public async Task<Customer?> GetByIdAsync(int id)
       {
           return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
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
           modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress);
       }
   }