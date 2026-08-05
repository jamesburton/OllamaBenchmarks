using System;
using Microsoft.EntityFrameworkCore;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set;} => base->Products;
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) {}
}

public static class ProductOperations
{
    public static async int ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var products = db.Products.Where(p => p.Category == category).ToList();

        if (products.Count() == 0)
            return 0;

        int updated = 0;
        foreach (var product in products)
        {
            // Apply the discount to the price
            product.Price = product.Price * (1 - discountPct);
            // Update the last modified timestamp
            product.LastModified = DateTime.UtcNow;
            product.Add(); // Mark the object for change
            updated++;
        }

        await db.SaveChangesAsync();
        return updated;
    }
}