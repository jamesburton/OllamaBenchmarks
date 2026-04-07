using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
        var parts = s.Split(':');
        if (parts.Length != 2) throw new FormatException("Invalid format for Money conversion");
        decimal amount = decimal.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
        string currency = parts[1];
        return new Money(amount, currency);
    }
}

public class ProductRepository
{
    private readonly ProductDbContext context;

    public ProductRepository(ProductDbContext context)
    {
        this.context = context;
    }

    public async Task<Product> SaveAsync(Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
        var parts = s.Split(':');
        if (parts.Length != 2) throw new FormatException("Invalid format for Money conversion");
        decimal amount = decimal.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
        string currency = parts[1];
        return new Money(amount, currency);
    }
}

public class ProductRepository
{
    private readonly ProductDbContext context;

    public ProductRepository(ProductDbContext context)
    {
        this.context = context;
    }

    public async Task<Product> SaveAsync(Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}