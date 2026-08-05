using System;
using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Money Price { get; set; }

    public Product()
    {
        Price = new Money(0m, "USD");
    }
}

public class ProductDbContext : DbContext
{
    private readonly DbContextOptions<ProductDbContext> _options;

    public DbSet<Product> Products { get; set; }

    public static Money ParseMoney(string s)
    {
        var parts = s.Split(':');
        if (parts.Length != 2) throw new ArgumentException("Invalid format");
        return new Money(decimal.Parse(parts[0]), parts[1]);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price).HasConversion(m => $"{m.Amount}:{m.Currency}", s => ParseMoney(s));
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
        await using var transaction = _context.Database.BeginTransaction();
        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return await _context.Products.FindAsync(product.Id);
        }
        finally
        {
            transaction.Rollback();
        }
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product != null) return product;
        return null;
    }
}