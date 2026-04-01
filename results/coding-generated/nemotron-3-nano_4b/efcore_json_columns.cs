using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

class Address {
    public string Street { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
}

class Company {
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HeadquartersAddress { get; set; }
}

class CompanyDbContext : DbContext {
    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Company>()
            .OwnsOne(c => c.HeadquartersAddress, b => { b.ToJson(); });
    }

    public DbSet<Company> Companies { get; set; }
}

static class CompanyQueries {
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city) {
        return db.Companies.Where(c => c.HeadquartersAddress.City == city).ToList();
    }
}