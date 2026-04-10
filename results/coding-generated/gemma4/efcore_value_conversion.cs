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
            throw new FormatException("Money string must be in 'amount:currency' format.");
        }

        if (!decimal.TryParse(parts[0], out var amount))
        {
            throw new FormatException("Invalid amount format.");
        }

        return new Money(amount, parts[1]);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure value conversion for Product.Price
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasConversion(
                // Convert to database value (string)
                m => $"{m.Amount}:{m.Currency}", 
                // Convert from database value (string)
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
        // Using AsNoTracking() ensures EF Core reads the raw data, 
        // but the conversion logic (HasConversion) is still applied upon materialization.
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}