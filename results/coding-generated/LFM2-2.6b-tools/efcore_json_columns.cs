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

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>().OwnsOne(c => c.HeadquartersAddress, b => b.ToJson());
    }

    public static class CompanyQueries
    {
        public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
        {
            return db.Companies.Where(c => c.HeadquartersAddress?.City == city).ToList();
        }
    }
}

public static class AddressJsonConverter : IJsonConverter<Address, string>
{
    public string Convert(Type type, object value, JsonWriter writer)
    {
        return value?.ToString();
    }
    public object ConvertFrom(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        return reader.ReadAsString();
    }
}

// Add this to the JSON serializer configuration if not already present:
public static class JsonSettings
{
    public static void RegisterJsonSerializationOptions(IServiceCollection services)
    {
        services.AddJsonSerializerOptions(options =>
        {
            options.Converters.Add(new AddressJsonConverter());
        });
    }
}

// In Program.cs or Startup.cs (depending on project setup):
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CompanyDbContext>(options => options.UseInMemoryDatabase("CompanyDb"));
builder.Services.AddSingleton<JsonSettings>();
builder.Services.AddDbContext<CompanyDbContext>(options => options.ConfigureJsonSerialization(builder.Services.BuildServiceProvider().GetRequiredService<JsonSettings>()));

// Example usage:
var companyContext = builder.Services.BuildServiceProvider().GetRequiredService<CompanyDbContext>();
var companies = CompanyQueries.FindCompaniesByCity(companyContext, "London");