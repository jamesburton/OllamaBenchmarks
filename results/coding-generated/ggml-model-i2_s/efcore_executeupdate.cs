using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EFCore10
{
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
        public DbSet<Product> Products { get; set; } = new DbSet<Product>();

        public ProductDbContext(DbContextOptions<ProductDbContext> options)
        {
            _options = options;
        }
    }

    public static class ProductOperations
    {
        public async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
        {
            var products = await db.Products
                .Where(p => p.Category == category)
                .OrderBy(p => p.Id)
                .Select(p => new { Id = p.Id, Name = p.Name, Price = p.Price * (1 - discountPct), LastModified = DateTime.UtcNow });

            var updatedCount = await products
                .Where(p => p.LastModified == DateTime.UtcNow)
                .CountAsync();

            return updatedCount;
        }
    }
}