using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

// 1. Money record
public record Money(decimal Amount, string Currency);

// 2. Product class
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Money Price { get; set; } = new Money(0, "USD");
}

// 3. ProductDbContext class
public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    // Helper method to parse the string format "amount:currency"
    private static Money ParseMoney(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return new Money(0, "USD");
        }
        var parts = s.Split(':');
        if (parts.Length == 2 && decimal.TryParse(parts[0], out decimal amount))
        {
            return new Money(amount, parts[1]);
        }
        // Fallback if parsing fails
        return new Money(0, "USD");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure value conversion for Product.Price
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasConversion(m => $"{m.Amount}:{m.Currency}", s => ParseMoney(s));
    }
}

// 4. ProductRepository class
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
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}