using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 3. Configure HeadquartersAddress as an owned JSON column
        modelBuilder.Entity<Company>()
            .OwnsOne(c => c.HeadquartersAddress, b => { b.ToJson(); });
    }
}

// 4. Static class CompanyQueries
public static class CompanyQueries
{
    public static async Task<List<Company>> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        // Querying based on the owned property's property (City)
        var companies = await db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToListAsync();

        return companies;
    }
}

// Example usage (optional, for completeness, not strictly required by prompt)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup DbContext (Example using in-memory for demonstration)
        var options = new DbContextOptionsBuilder<CompanyDbContext>()
            .UseInMemoryDatabase("CompanyDb")
            .Options;

        using (var context = new CompanyDbContext(options))
        {
            context.Database.EnsureCreated();

            // Seed Data
            context.Companies.Add(new Company { Name = "Alpha Corp", HeadquartersAddress = new Address { City = "London", Street = "1 Main St" } });
            context.Companies.Add(new Company { Name = "Beta Ltd", HeadquartersAddress = new Address { City = "Manchester", Street = "2 High Rd" } });
            context.Companies.Add(new Company { Name = "Gamma Inc", HeadquartersAddress = new Address { City = "London", Street = "3 Park Ave" } });

            // Test Query
            Console.WriteLine("Finding companies in London:");
            var londonCompanies = await CompanyQueries.FindCompaniesByCity(context, "London");
            foreach (var company in londonCompanies)
            {
                Console.WriteLine($"ID: {company.Id}, Name: {company.Name}, City: {company.HeadquartersAddress.City}");
            }
        }
    }
}
*/