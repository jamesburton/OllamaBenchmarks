using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        // Get all products in the specified category
        var products = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        // Apply the discount to each product
        foreach (var product in products)
        {
            product.Price = product.Price * (1 - discountPct);
            product.LastModified = DateTime.UtcNow;
        }

        // Save changes and return the number of affected rows
        return await db.SaveChangesAsync();
    }
}