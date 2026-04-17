using Microsoft.EntityFrameworkCore;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("CustomerDatabase");
    }
}

public static class CustomerRepository
{
    public static Customer Update(CustomerContext context, int id, string newEmail)
    {
        var customer = context.Customers.Find(id);

        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {id} was not found.");
        }

        customer.Email = newEmail;
        context.SaveChanges();

        return customer;
    }
}