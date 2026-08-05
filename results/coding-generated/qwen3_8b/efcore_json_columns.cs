class Address
{
    public string Street { get; set; }
    public string City { get; set}
    public string PostCode { get; set; }
}

class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HeadquartersAddress { get; set; }
}

class CompanyDbContext : DbContext
{
    public DbSet<Company> Companies { get; set; }

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Company>().OwnsOne(c => c.HeadquartersAddress, b => { b.ToJson(); });
    }
}

static class CompanyQueries
{
    public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
    {
        return db.Companies
            .Where(c => c.HeadquartersAddress?.City == city)
            .ToList();
    }
}