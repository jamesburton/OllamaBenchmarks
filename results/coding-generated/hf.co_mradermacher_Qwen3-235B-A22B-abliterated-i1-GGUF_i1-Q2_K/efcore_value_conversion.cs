using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Money Price { get; set; } = new Money(0, "USD");
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasConversion(
                m => $"{m.Amount}:{m.Currency}",
                s => ParseMoney(s)
            );
    }

    private static Money ParseMoney(string s)
    {
        var parts = s.Split(':');
        var amount = decimal.Parse(parts[0]);
        var currency = parts[1];
        return new Money(amount, currency);
    }
}

public class ProductRepository
{
    private readonly ProductDbContext db;

    public ProductRepository(ProductDbContext context)
    {
        db = context;
    }

    public async Task<Product> SaveAsync(Product product)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}