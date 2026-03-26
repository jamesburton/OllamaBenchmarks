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
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
}

public static class ProductOperations
{
    public async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        int count = 0;
        foreach (var product in db.Products
        {
            if (product.Category == category)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
                count++;
            }
        }
        await db.SaveChangesAsync();
        return count;
    }
}