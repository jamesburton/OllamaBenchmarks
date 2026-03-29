using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

// Address class - simple POCO with public get/set properties
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
}

// Company entity with owned JSON Address
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HeadquartersAddress { get; set; }
}

// DbContext for Company entities
public class CompanyDbContext : DbContext
{
    public DbSet<Company> Companies { get; set; }

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure HeadquartersAddress as an owned JSON column
        builder.Entity<Company>()
            .OwnsOne(c => c.HeadquartersAddress, b =>
            {
                b.ToJson();
            });
    }
}

// Static class containing query methods
public static class CompanyQueries
{
    /// <summary>
    /// Finds companies by their headquarters city.
    /// </summary>
    /// <param name="db">The CompanyDbContext instance.</param>
    /// <param name="city">The city to search for.</param>
    /// <returns>A list of companies matching the city.</returns>
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        return db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToList();
    }
}