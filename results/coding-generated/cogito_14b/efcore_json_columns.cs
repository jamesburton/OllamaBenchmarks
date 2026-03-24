using Microsoft.EntityFrameworkCore;

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
    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) 
        : base(options) { }

    public DbSet<Company> Companies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>()
            .Property(c => c.HeadquartersAddress)
            .HasJsonConversion();
    }
}

public static class CompanyQueries
{
    public static async Task<List<Company>> FindCompaniesByCity(
        CompanyDbContext db, 
        string city)
    {
        return await db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToListAsync();
    }
}