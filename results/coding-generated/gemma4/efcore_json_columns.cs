using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Address class (No attributes)
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

// 3. DbContext implementation
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
            .OwnsOne(c => c.HeadquartersAddress, b => { 
                b.ToJson(); 
            });
    }
}

// 4. Static query class
public static class CompanyQueries
{
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        // Querying the JSON nested property using the dot notation
        var companies = db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToList();

        return companies;
    }
}