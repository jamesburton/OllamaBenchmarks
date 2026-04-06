using Microsoft.EntityFrameworkCore;

public record Money(decimal Amount, string Currency);

public class Product


    public int Id { get; set; }
    public string Name { get; set; }
    public Money Price { get; set; } = new Money(0, "USD");

    public Product(int id, string name, Money price)
        : this(id, name)

    {
        Price = price;
    }

    public Money ParseMoney(string s)


    {
        var parts = s.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) throw new ArgumentException("Invalid money format");
        var amountStr = parts[0];
        var currency = parts[1];
        return Money.Parse(amountStr, currency);
    }


public class ProductDbContext : DbContext


    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)

    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)


    {
        modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(m => $"{m.Amount}:{m.Currency}", ParseMoney);
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
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> GetByIdAsync(int id)


        {
            return await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

    }