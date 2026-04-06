using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);

public class Product
{
    public int Id { get; set; }

    private string _name;
    public string Name 
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(Name));
    }

    private Money _price = new(0, "USD");
    public Money Price
    {
        get => _price;
        set => _price = value ?? throw new ArgumentNullException(nameof(Price));
    }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {

    }

    private static Money ParseMoney(string s)
    {
        var parts = s.Split(':');
        return new Money(decimal.Parse(parts[0]), parts.Length > 1 ? parts[1] : "USD");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(
            m => $"{m.Amount}:{m.Currency}",
            s => ParseMoney(s)
        );
    }
}

public class ProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context) => _context = context;

    public async Task<Product> SaveAsync(Product product)
    {
        if (product.Id == 0)
        {
            await _context.Products.AddAsync(product);
        }
        else
        {
            var existingProduct = await _context.Products.FindAsync(product.Id);
            if (existingProduct != null)
            {
                _context.Entry(existingProduct).CurrentValues.SetValues(product);
            }
        }

        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id) => 
        await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
}
</generated_code>