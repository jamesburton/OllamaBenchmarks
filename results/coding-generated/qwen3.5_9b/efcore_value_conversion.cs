using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public Money Price { get; set; } = new Money(0, "USD");
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;

    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
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
        return new Money(decimal.Parse(parts[0]), parts[1]);
    }
}

public class ProductRepository
{
    public ProductRepository(ProductDbContext context)
    {
    }

    public Task<Product> SaveAsync(Product product)
    {
        context.Products.Add(product);
        return context.SaveChangesAsync() > 0 ? Task.FromResult(product) : throw new InvalidOperationException("Save failed");
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        return context.Products.FindAsync(id).AsNoTracking();
    }
}