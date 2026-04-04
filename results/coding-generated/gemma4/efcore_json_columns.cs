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

// 3. CompanyDbContext class
public class CompanyDbContext : DbContext
{
    public DbSet<Company> Companies { get; set; }

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure HeadquartersAddress as an owned JSON column
        modelBuilder.Entity<Company>()
            .OwnsOne(c => c.HeadquartersAddress, b => { b.ToJson(); });
    }
}

// 4. CompanyQueries static class
public static class CompanyQueries
{
    /// <summary>
    /// Queries companies where the HeadquartersAddress.City matches the given city parameter.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="city">The city name to filter by.</param>
    /// <returns>A list of matching companies.</returns>
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        // This query leverages EF Core's ability to query nested JSON properties
        // configured via OwnsOne().ToJson().
        return db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToList();
    }
}