using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

// 1. Money Record
public record Money(decimal Amount, string Currency);

// 2. Product Class
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Money Price { get; set; } = new Money(0, "USD"); // Initialized to avoid null issues
}

// 3. ProductDbContext
public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    private static Money ParseMoney(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return new Money(0, "USD");
        }

        var parts = s.Split(':');
        if (parts.Length == 2 && decimal.TryParse(parts[0], out decimal amount))
        {
            return new Money(amount, parts[1]);
        }

        // Fallback or error handling if format is incorrect
        return new Money(0, "USD");
    }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure value conversion for the Money type
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasConversion(
                // Convert Money record to string for storage
                m => $"{m.Amount}:{m.Currency}",
                // Convert string from storage back to Money record
                s => ParseMoney(s)
            );
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

    public async Task<Product> SaveAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking() // Use AsNoTracking() for read-only operations
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}

// Example Usage (Optional, for completeness, not required by prompt but useful for context)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup DbContext (using in-memory for demonstration)
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: "ProductDb")
            .Options;

        using (var context = new ProductDbContext(options))
        {
            context.Database.EnsureCreated();

            var repository = new ProductRepository(context);

            // Create and Save
            var newProduct = new Product
            {
                Name = "Laptop",
                Price = new Money(1250.50m, "EUR")
            };
            await repository.SaveAsync(newProduct);

            // Retrieve
            var retrievedProduct = await repository.GetByIdAsync(newProduct.Id);

            if (retrievedProduct != null)
            {
                Console.WriteLine($"Retrieved Product: {retrievedProduct.Name}");
                Console.WriteLine($"Price: {retrievedProduct.Price.Amount} {retrievedProduct.Price.Currency}");
            }
        }
    }
}
*/