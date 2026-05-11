using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;

// 1. Address class
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
}

// 2. Company entity class
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HeadquartersAddress { get; set; }
}

// 3. CompanyDbContext class
public class CompanyDbContext : DbContext
{
    public DbSet<Company> Companies { get; set; }

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure HeadquartersAddress as an owned JSON column
        modelBuilder.Entity<Company>()
            .OwnsOne(c => c.HeadquartersAddress, b =>
            {
                // Use ToJson() to configure the property as a JSON column
                b.ToJson();
            });
    }
}

// 4. Static class for queries
public static class CompanyQueries
{
    /// <summary>
    /// Finds companies where the HeadquartersAddress City matches the given city parameter.
    /// </summary>
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        return db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToList();
    }
}

// Example usage (optional, for completeness, not required by prompt)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup DbContext (using in-memory for demonstration)
        var options = new DbContextOptionsBuilder<CompanyDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using (var context = new CompanyDbContext(options))
        {
            context.Database.EnsureCreated();

            // Seed data
            context.Companies.Add(new Company
            {
                Name = "Alpha Corp",
                HeadquartersAddress = new Address { City = "London" }
            });
            context.Companies.Add(new Company
            {
                Name = "Beta Inc",
                HeadquartersAddress = new Address { City = "Manchester" }
            });
            context.Companies.Add(new Company
            {
                Name = "Gamma Ltd",
                HeadquartersAddress = new Address { City = "London" }
            });

            await context.SaveChangesAsync();

            // Query using the static method
            var londonCompanies = CompanyQueries.FindCompaniesByCity(context, "London");
            Console.WriteLine($"Found {londonCompanies.Count} companies in London.");

            var manchesterCompanies = CompanyQueries.FindCompaniesByCity(context, "Manchester");
            Console.WriteLine($"Found {manchesterCompanies.Count} companies in Manchester.");
        }
    }
}
*/