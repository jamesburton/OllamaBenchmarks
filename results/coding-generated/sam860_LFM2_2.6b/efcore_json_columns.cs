using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
}

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HeadquartersAddress { get; set; }
}

public class CompanyDbContext : DbContext
{
    public DbSet<Company> Companies { get; set; }

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>()
            .OwnsOne(c => c.HeadquartersAddress, b => b.ToJson());
    }

    public static class CompanyQueries
    {
        public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
        {
            return db.Companies
                .Where(c => c.HeadquartersAddress?.City == city)
                .ToList();
        }

        private static string ToJson(this EntityTypeBuilder<Address> builder)
        {
            var json = JsonSerializer.Serialize(builder.GetValue<Address>().ToDictionary(q => q.Key, q => q.Value));
            builder.SetValue<object>("PostCode", json);
            return json;
        }
    }
}

// Note: The ToJson method is a helper to serialize the Address object to JSON for the OwnsOne configuration.
// Since EF Core does not natively support owned JSON columns with a custom converter, this workaround
// serializes the Address dictionary to a JSON string and stores it in the JSON column.