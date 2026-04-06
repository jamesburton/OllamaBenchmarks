using Microsoft.EntityFrameworkCore;

public class Money { public record Money(decimal Amount, string Currency); }

public class Product { public int Id { get; set; } public string Name { get; set; } public Money MoneyPrice { get; set; } }

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    private static Money ParseMoney(string s)
    {
        var parts = s.Split(':');
        return new Money(decimal.Parse(parts[0]), parts[1]);
    }

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasConversion(m => m => ParseMoney(m));
    }
}

public class ProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context) : base(context) { }

    public Task<Product> SaveAsync(Product product)
    {
        var newProduct = new Product { Id = product.Id, Name = product.Name, MoneyPrice = new Money(0, "USD") };
        _context.Entities.Add(newProduct);
        return await _context.SaveChanges();
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        return _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
    }
}