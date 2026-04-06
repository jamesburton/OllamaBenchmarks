using Microsoft.EntityFrameworkCore;

// 1. Money Record
public record Money(decimal Amount, string Currency);

// 2. Product Class
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Money Price { get; set; } = new Money(0, "USD");
}

// 3. ProductDbContext
public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasConversion(
                m => $"{m.Amount}:{m.Currency}",
                s => ParseMoney(s)
            );
    }

    private static Money ParseMoney(string s)
    {
        var parts = s.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.TrimEntries);
        return new Money(decimal.Parse(parts[0]), parts[1]);
    }
}

// 4. ProductRepository
public class ProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> SaveAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }
}