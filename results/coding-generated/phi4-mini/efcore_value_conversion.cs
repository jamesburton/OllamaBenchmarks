using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Money Price { get; private set; } = new Money(0, "USD");
}

public partial class ProductDbContext : DbContext
{
    public virtual DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(m => $"{m.Amount}:{m.Currency}", s => ParseMoney(s));
    }

    private static Money ParseMoney(string s)
    {
        var parts = s.Split(':');
        if (parts.Length == 2 && decimal.TryParse(parts[0], out decimal amount))
        {
            return new Money(amount, parts[1]);
        }
        throw new ArgumentException("Invalid money format", nameof(s));
    }

    public override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.Options.ContainsKey(nameof(ProductDbContext.DatabaseName)))
        {
            using var serviceProvider = ServiceCollection.Defaults;
            ConfigureService(serviceProvider);
        }
    }
}

public class ProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product> SaveAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        return await _context.SaveChangesAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return product;
    }
}