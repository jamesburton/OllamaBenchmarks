using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using System.Linq;
using System;
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
    /// <summary>
    /// Queries companies where HeadquartersAddress.City matches the given city parameter.
    /// </summary>
    public static async Task<List<Company>> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        // EF Core 10 Left Join equivalent for querying owned types requires specific navigation or projection.
        // Since we are using Owned types with ToJson(), EF Core handles the JSON mapping.
        // To query based on a property within an owned type, we typically need to access the navigation property.

        var results = await db.Companies
            .Where(c => c.HeadquartersAddress != null && c.HeadquartersAddress.City == city)
            .ToListAsync();

        return results;
    }
}