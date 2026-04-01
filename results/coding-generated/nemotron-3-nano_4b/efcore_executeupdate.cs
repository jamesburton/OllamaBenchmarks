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
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) => InitializeOptions(options);

    private void InitializeOptions(DbContextOptions<ProductDbContext> options) => base.Initialize(options);
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        decimal discountFactor = 1.0m - discountPct;
        foreach (var product in db.Products.Where(p => p.Category == category))
        {
            product.Price = product.Price * discountFactor;
            product.LastModified = DateTime.UtcNow;
        }
        int affected = await db.SaveChangesAsync();
        return affected;
    }
}