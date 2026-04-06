using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Money Price { get; set; } = new Money(0, "USD");
}

public static class ProductDbContextExtensions
{
    private static Money ParseMoney(string s)
    {
        var parts = s.Split(':');
        if (parts.Length != 2) throw new ArgumentException("Invalid format for money value.");
        return new Money(decimal.Parse(parts[0]), parts[1]);
    }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasConversion(
                m => $"{m.Amount}:{m.Currency}",
                s => ProductDbContextExtensions.ParseMoney(s));
    }
}

public class ProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product> SaveAsync(Product product)
    {
        await _context.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }
}