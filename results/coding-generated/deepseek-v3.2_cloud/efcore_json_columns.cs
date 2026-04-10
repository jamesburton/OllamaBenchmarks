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

public class CompanyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public CompanyDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<CompanyDbContext> options)
        : base(options)
    {
    }

    public Microsoft.EntityFrameworkCore.DbSet<Company> Companies { get; set; }

    protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder builder)
    {
        builder.Entity<Company>().OwnsOne(c => c.HeadquartersAddress, b =>
        {
            b.ToJson();
        });
    }
}

public static class CompanyQueries
{
    public static System.Collections.Generic.List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        return db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToList();
    }
}