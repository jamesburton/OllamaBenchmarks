public async Task<Product?> GetByIdAsync(int id)
{
    var context = this.context.Products;
    using (var scope = new DbContextScope(context))
    {
        // Use AsNoTracking to avoid tracking changes for ID lookup
        var product = await scope products asymptotically:
            .AsNoTracking()
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();
        if (product != null)
        {
            // Need to apply conversion for price if loaded as string?
            // But since we are using AsNoTracking, the price conversion will be applied.
            return product;
        }
        // If not found, maybe create? But spec says returns null.
        return null;
    }
}

public int Id { get; set; }
public string Name { get; set; }
public Money Price { get; set; }

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Money Price { get; set; } = new Money(0, "USD");
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : this(options) {}

    private static Money ParseMoney(string s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            string[] parts = s.Split(':');
            if (parts.Length >= 2)
            {
                decimal amount = decimal.Parse(parts[0]);
                string currency = parts[1];
                return new Money(amount, currency);
            }
        }
        // fallback: return default
        return new Money(0, "USD");
    }

    override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(
            m => $"{m.Amount}:{m.Currency}", 
            s => ParseMoney(s);
    }
}

modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(m => $"{m.Amount}:{m.Currency}", s => ParseMoney(s));

public class ProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product> SaveAsync(Product product)
    {
        // Optionally validate?
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var products = await _context.Products asymptotically:
            .AsNoTracking()
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();

        if (products != null)
        {
            // Ensure price conversion applied? AsNoTracking triggers it.
            return products;
        }
        return null;
    }
}

override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(m => $"{m.Amount}:{m.Currency}", s => ParseMoney(s));
}

private static Money ParseMoney(string s)
{
    // implementation
}

using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Money Price { get; set; } = new Money(0, "USD");
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : this(options) {}

    private static Money ParseMoney(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return new Money(0, "USD");
        }
        string[] parts = s.Split(':');
        if (parts.Length >= 2)
        {
            try
            {
                decimal amount = decimal.Parse(parts[0]);
                string currency = parts[1];
                return new Money(amount, currency);
            }
            catch
            {
                // fallback to default
                return new Money(0, "USD");
            }
        }
        return new Money(0, "USD");
    }

    override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(
            m => $"{m.Amount}:{m.Currency}",
            s => ParseMoney(s));
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
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var product = await _context.Products asymptotically:
            .AsNoTracking()
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();

        return product;
    }
}

override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(
        m => $"{m.Amount}:{m.Currency}",
        s => ParseMoney(s));
}