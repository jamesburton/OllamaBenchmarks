using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

// 1. Money Record
public record Money(decimal Amount, string Currency);

// 2. Product Class
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Price must be initialized to avoid null issues
    public Money Price { get; set; } = new Money(0, "USD");
}

// 3. ProductDbContext Class
public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    // Private static method for value conversion
    private static Money ParseMoney(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return new Money(0, "USD");
        }

        var parts = s.Split(':');
        if (parts.Length != 2)
        {
            // Handle malformed string gracefully
            return new Money(0, "USD");
        }

        if (!decimal.TryParse(parts[0], out var amount))
        {
            return new Money(0, "USD");
        }

        return new Money(amount, parts[1]);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the value conversion for Product.Price
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasConversion(
                // Converter to database type (string)
                m => $"{m.Amount}:{m.Currency}", 
                // Converter from database type (string)
                s => ParseMoney(s)
            );
    }
}

// 4. ProductRepository Class
public class ProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds and saves the product to the database.
    /// </summary>
    public async Task<Product> SaveAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <summary>
    /// Retrieves a product by ID without tracking changes.
    /// </summary>
    public async Task<Product?> GetByIdAsync(int id)
    {
        // Using AsNoTracking() ensures EF Core re-applies the conversion logic 
        // when loading the object from the database.
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}