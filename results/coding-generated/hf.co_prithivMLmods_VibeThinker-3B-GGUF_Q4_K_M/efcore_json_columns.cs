using System;
using System.Collections.Generic;

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

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) {
        // EF Core 10 or later
    }

    protected virtual void OnModelCreating(ModelBuilder modelBuilder) when (modelBuilder.Entity<Company>().IsOneToMany())
    {
        modelBuilder.Entity<Company>().OwnsOne(c => c.HeadquartersAddress, b => { b.ToJson(); });
    }
}

public static class CompanyQueries
{
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        return db COMPANIES
            .Where(c => c.HeadquartersAddress.City == city)
            .ToList();
    }
}

using System;
using System.Collections.Generic;

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

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
    {
        // EF Core constructor placeholder
    }

    protected virtual void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>()
            .OwnsOne(c => c.HeadquartersAddress, b => { b.ToJson(); });
    }
}

public static class CompanyQueries
{
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        return db.COMMANDE
            .Where(c => c.HeadquartersAddress.City == city)
            .ToList();
    }
}

var queries = db.MyModels.Where(m => m.Name > 10).ToList();

var queries = db.MyModels.Where(m => m.Name > 10);

public class MyEntity { public DbSet<MyEntity> MySets { get; set; } }

var queries = dbMyModels.Where(m => m.Name > 10).ToList();

public class MyEntity { public DbSet<MyEntity> MySets { get; set; } }

var queries = dbMyModels.Where(m => m.Name > 10);

using System;
using System.Collections.Generic;

public class Company { ... }

public class CompanyDbContext : DbContext
{
    public DbSet<Company> Companies { get; set; }

    // ...
}

public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
{
    return db.companies.Where(c => c.HeadquartersAddress.City == city).ToList();
}

var queries = dbMyModels.Where(m => m.Name > 10);

var queries = dbMyModels.Where(m => m.Name > 10);

dbMyModels
    .Where(m => m.IsActive)
    .ToList();

var result =;

var companies = db.COMANDE; // This might not compile

var queries = dbMyModels.Where(m => m.Name > 10);

public class MyEntity { public DbSet<MyEntity> MySets { get; set; } }

public class Program : System.Collections.Generic.CollectionBase
{
    public static List<MyEntity> MyList()
    {
        return db.MySets.Where(s => s.Name > 10).ToList();
    }
}