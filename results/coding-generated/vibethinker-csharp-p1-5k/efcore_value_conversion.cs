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

    public ProductDbContext(DbContextOptions<ProductDbContext> options) => base options;

    private static Money ParseMoney(string s)
    {
        if (string.IsNullOrEmpty(s))
            throw new ArgumentException("String cannot be null or empty.");
        var parts = s.Split(':');
        if (parts.Length != 2)
            throw new ArgumentException("String must contain exactly one colon to separate amount and currency.");
        return new Money(decimal.Parse(parts[0]), parts[1]);
    }

    public override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(m => $"{m.Amount}:{m Currency}", s => ParseMoney(s));
}

public class ProductRepository
{
    private readonly ProductDbContext context;

    public ProductRepository(ProductDbContext context)
    {
        this.context = context;
    }

    public Task<Product> SaveAsync(Product product) => await context.Add(product).SaveChangesAsync();

    public Task<Product?> GetByIdAsync(int id) => await context.Products
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id);
    }
}