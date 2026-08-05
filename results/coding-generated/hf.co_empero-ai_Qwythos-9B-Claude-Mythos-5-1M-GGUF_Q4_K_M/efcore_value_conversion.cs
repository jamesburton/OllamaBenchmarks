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

    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    private static Money ParseMoney(string s) =>
        new Money(decimal.Parse(s.Split(':')[0]), s.Split(':')[1]);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(
            m => $"{m.Amount}:{m.Currency}",
            s => ParseMoney(s));
    }
}

public class ProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public Task<Product> SaveAsync(Product product)
    {
        _context.Products.Add(product);
        return _context.SaveChangesAsync();
    }

    public Task<Product?> GetByIdAsync(int id) =>
        _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
}