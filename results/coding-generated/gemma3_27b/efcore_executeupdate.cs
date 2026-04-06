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

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        if (db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            var productsToUpdate = db.Products.Where(p => p.Category == category).ToList();
            int updatedCount = 0;

            foreach (var product in productsToUpdate)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
                updatedCount++;
            }

            await db.SaveChangesAsync();
            return updatedCount;
        }
        else
        {
            return await db.Products
                .Where(p => p.Category == category)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Price, p.Price * (1 - discountPct))
                    .SetProperty(p => p.LastModified, DateTime.UtcNow));
        }
    }
}