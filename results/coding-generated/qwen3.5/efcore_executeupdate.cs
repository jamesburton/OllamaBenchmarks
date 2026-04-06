using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.InMemory;

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

        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }
    }

    public static class ProductOperations
    {
        public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
        {
            var products = db.Products
                .Where(p => p.Category == category)
                .ToList();

            foreach (var product in products)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            return products.Count;
        }
    }

using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.InMemory;

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

        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }
    }

    public static class ProductOperations
    {
        public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
        {
            var products = db.Products
                .Where(p => p.Category == category)
                .ToList();

            foreach (var product in products)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            return products.Count;
        }
    }

.
    I will wrap it in