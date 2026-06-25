using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);

public class Product 
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Money Price { get; set; } = new(0m, "USD");
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = default!;

    public ProductDbContext(DbContextOptions<ProductDbContext> options) 
        : base(options)
    {
    }

    private static Money ParseMoney(string s)
    {
        var parts = s.Split(':');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid money format", nameof(s));

        return new Money(decimal.Parse(parts[0]), parts[1]);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasConversion(
                m => $"{m.Amount}:{m.Currency}",
                s => ParseMoney(s));
    }
}

public class ProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context) 
        => _context = context;

    public async Task<Product> SaveAsync(Product product)
    {
        if (product.Id == 0)
            _context.Products.Add(product);
        else
            _context.Entry(product).State = EntityState.Modified;

        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id) 
        => await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
}