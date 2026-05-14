using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Money Price { get; set; } = new Money(0, "USD");
}
public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }
    private static Money ParseMoney(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return new Money(0, "USD");
        var parts = s.Split(new[] { ':' }, 2);
        if (parts.Length == 0) return new Money(0, "USD");
        var amount = decimal.Parse(parts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
        var currency = parts.Length > 1 ? parts[1] : "USD";
        return new Money(amount, currency);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasConversion(m => $"{m.Amount}:{m.Currency}", s => ParseMoney(s));
    }
}
public class ProductRepository
{
    public ProductRepository(ProductDbContext context) => Context = context;
    private ProductDbContext Context { get; }
    public Task<Product> SaveAsync(Product product)
    {
        Context.Products.Add(product);
        await Context.SaveChangesAsync();
        return product;
    }
    public Task<Product?> GetByIdAsync(int id)
    {
        return Context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }
}