using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public class Address {
    public string Street { get; set; }
    public string City { get; set; }
}

[ComplexType]
public class Company {
    public int Id { get; set; }
    public string Name { get; set; }

    [ComplexType]
    public class HeadquartersAddress : Address {
        // Empty constructor for JSON conversion
        private HeadquartersAddress() {}

        public HeadquartersAddress(string street, string city) 
            : base(street, city) {}
    }
}

public class CompanyDbContext : DbContext {
    public DbSet<Company> Companies { get; set; }

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
        : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Company>()
            .Property(c => c.HeadquartersAddress)
            .HasConversion<HeadquartersAddress, string>(
                s => new HeadquartersAddress(s.Street, s.City),
                s => new Company(s));
    }
}

public static class CompanyQueries {
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city) {
        return db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToList();
    }
}