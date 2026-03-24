using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>()
            .OwnsOne(c => c.HeadquartersAddress, a =>
            {
                a.Property(p => p.Street).HasColumnType("nvarchar(max)");
                a.Property(p => p.City).HasColumnType("nvarchar(max)");
                a.Property(p => p.PostCode).HasColumnType("nvarchar(max)");
            });
    }
}

public static class CompanyQueries
{
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        return db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToList();
    }
}