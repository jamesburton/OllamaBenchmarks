using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Money Price = new Money(0, "USD");
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    private static string ParseMoney(string s)
    {
        var parts = s.Split(':');
        if (parts.Length != 2)
            return s;
        if (!decimal.TryParse(parts[0], out decimal amount))
            return s;
        return new Money(amount, parts[1]);
    }

    public override void OnModelCreating(ModelBuilder modelBuilder)
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
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}