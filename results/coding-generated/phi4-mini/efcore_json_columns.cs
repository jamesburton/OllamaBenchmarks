using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

public class Address {
    public string Street { get; set; }
    public string City { get; set; }
    public string PostCode { get; set; }
}

public class Company {
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    // Owned JSON property for HeadquartersAddress
    private Address _headquartersAddress;

    public Address HeadquarterssAddress => _headquartersAddress;

    public void SetHeadquartersAddress(Address address) {
        _headquartersAddress = address;
    }
}

public class CompanyDbContext : DbContext {
    public DbSet<Company> Companies { get; set; }

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        // Configure HeadquartersAddress as an owned JSON column
        builder.Entity<Company>().OwnsOne(c => c.HeadquartersAddress, b =>
            {
                b.ToJson();
            });
    }
}

public static class CompanyQueries {
    public static async Task<List<Company>> FindCompaniesByCity(CompanyDbContext db, string city) {
        return await db.Companies
            .Where(c => c.HeadquartersAddress.City == city)
            .ToListAsync();
    }
}