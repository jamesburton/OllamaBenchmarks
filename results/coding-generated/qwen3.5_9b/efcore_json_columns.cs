public class Address
{
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string PostCode { get; set; } = null!;
}

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public Address HeadquartersAddress { get; set; } = default!;
}

public class CompanyDbContext : DbContext
{
    public DbSet<Company> Companies => Set<Company>();

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>()
            .OwnsOne(c => c.HeadquartersAddress, b => 
            {
                b.ToJson();
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