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
        var affectedRows = await db.Products
            .Where(p => p.Category == category)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Price, p.Price * (1 - discountPct))
                .SetProperty(p => p.LastModified, DateTime.UtcNow));

        return affectedRows;
    }
}

// Example usage:
// var options = new DbContextOptionsBuilder<ProductDbContext>()
//     .UseInMemoryDatabase("TestDb")
//     .Options;
// var db = new ProductDbContext(options);
// var affectedRows = await ProductOperations.ApplyDiscount(db, "Electronics", 0.1m);
// Console.WriteLine($"Rows affected: {affectedRows}");