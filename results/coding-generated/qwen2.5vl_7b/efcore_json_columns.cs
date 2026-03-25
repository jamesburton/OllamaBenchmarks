using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ComplexType]
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
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>()
            .Property(c => c.HeadquartersAddress)
            .HasJsonConversion();
    }
}

public static class CompanyQueries
{
    public static async Task<List<Company>> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        return await db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToListAsync();
    }
}

// Example usage
class Program
{
    static async Task Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<CompanyDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        using (var db = new CompanyDbContext(options))
        {
            var companies = await CompanyQueries.FindCompaniesByCity(db, "London");
            foreach (var company in companies)
            {
                Console.WriteLine($"Company Name: {company.Name}, Headquarters Address: {company.HeadquartersAddress.City}");
            }
        }
    }
}