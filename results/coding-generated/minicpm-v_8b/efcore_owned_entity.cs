using System;
using Microsoft.EntityFrameworkCore;

public class Address {
    public string Street { get; set; }
    public string City { get; set; }
}

public class Customer {
    public int Id { get; set; }
    public string Name { get; set; }

    [Owned]
    public Address HomeAddress { get; set; }
}

public class CustomerDbContext : DbContext {
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress);
    }

    public async Task<Customer> CreateAsync(Customer customer) {
        await base.SaveChangesAsync();
        return customer;
    }
}

public class CustomerRepository {
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context) {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(int id) {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
    }
}