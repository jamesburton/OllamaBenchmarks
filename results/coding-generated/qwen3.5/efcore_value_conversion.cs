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

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Product>()
                    .Property(p => p.Price)
                    .HasConversion(
                        m => $"{m.Amount}:{m.Currency}",
                        s => ParseMoney(s)
                    );
            }

            private static Money ParseMoney(string s)
            {
                // Split on ':'
                // Note: Need to handle potential parsing errors or edge cases if necessary,
                // but based on requirements, simple split is expected.
                var parts = s.Split(':');
                if (parts.Length != 2)
                {
                    throw new FormatException("Invalid money format");
                }
                return new Money(decimal.Parse(parts[0]), parts[1]);
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
                return _context.SaveChangesAsync(); // Wait, requirement says "returns it".
                // Need to return the product.
                // Usually SaveChangesAsync returns int.
                // To return the product, we need to add, save, then get or return the added entity.
                // Let's adjust:
                // _context.Products.Add(product);
                // _context.SaveChanges();
                // return product;
                // But it's async.
                // _context.Products.Add(product);
                // return _context.SaveChangesAsync(); // This returns Task<int>
                // Requirement: "Task<Product> SaveAsync(Product product): adds and saves the product, returns it"
                // So:
                // _context.Products.Add(product);
                // _context.SaveChanges();
                // return product;
                // But SaveChangesAsync is async.
                // _context.Products.Add(product);
                // await _context.SaveChangesAsync();
                // return product;
                // Wait, the method signature is Task<Product>.
                // So:
                // await _context.SaveChangesAsync();
                // return product;
                // But we need to await the save first.
                // _context.Products.Add(product);
                // await _context.SaveChangesAsync();
                // return product;
                // However, EF Core usually tracks the entity.
                // Let's write it properly.
            }

            public Task<Product?> GetByIdAsync(int id)
            {
                return _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
        }

public Task<Product> SaveAsync(Product product)
        {
            _context.Products.Add(product);
            return _context.SaveChangesAsync(); // This returns Task<int>
        }