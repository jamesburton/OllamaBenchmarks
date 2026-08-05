using System;
using System.Collections.Generic;

public class Address {
    public string Street { get; set; }
    public string City { get; set; }
}

[Owned]
public class HeadquartersAddress : Address {}

public class Company {
    public int Id { get; set; }
    public string Name { get; set; }

    [OwnsOne]
    public HeadquartersAddress HeadquartersAddress { get; set; }
}

public class CompanyDbContext : DbContext {
    public DbSet<Company> Companies { get; set; }

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder builder) {
        builder.Entity<Company>().OwnsOne(c => c.HeadquartersAddress, b => { b.ToJson(); });
    }
}

public static class CompanyQueries {
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city) {
        return db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToList();
    }
}