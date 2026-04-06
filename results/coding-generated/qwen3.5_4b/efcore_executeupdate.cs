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
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }

    public static class ProductOperations
    {
        public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
        {
            var products = await db.Products
                .Where(p => p.Category == category)
                .ToListAsync();

            int count = 0;
            foreach (var product in products)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
                count++;
            }

            await db.SaveChangesAsync();
            return count;
        }
    }