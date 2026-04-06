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
    public DbSet<Product> Products { get; set; } = default!;

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    private static Money ParseMoney(string s)
    {
        var parts = s.Split(':', 2);
        if (parts.Length != 2)
            throw new ArgumentException("Format must be 'amount:currency'");
        if (!decimal.TryParse(parts[0], out var amount))
            throw new FormatException("Amount must be a valid decimal");
        return new Money(amount, parts[1]);
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
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context) => _context = context;

    public async Task<Product> SaveAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }
}