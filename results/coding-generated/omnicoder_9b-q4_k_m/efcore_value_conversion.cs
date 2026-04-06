public record Money(decimal Amount, string Currency);

public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Money Price { get; set; } = new Money(0, "USD");
        }

public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
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
                // Split on ':'
                var parts = s.Split(':');
                var amount = decimal.Parse(parts[0]);
                var currency = parts[1];
                return new Money(amount, currency);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder))
            {
                modelBuilder.Entity<Product>( p => p.Property);
                modelBuilder.Entity<Product>().Property(p => p p => p.Price);
                modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(m => $"{m.Amount}:{m.Currency}", s => ParseMoney(s));
            }
        }

public class ProductRepository
        {
            private readonly ProductDbContext _context;

            public ProductRepository(ProductDbContext context)
            {
                _context = context;
            }

            public Task<Product> SaveAsync(Product
            {
                _context = _context.Add;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return product;
            }

            public Task<Product?> GetByIdAsync(int id)
            {
                return _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
        }

public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Money Price { get; set;; } = new Money(0, "USD"););


public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
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
                var parts = s.Split(':');
                return new Money(decimal.Parse(parts[0]), parts[1]);
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

            public ProductRepository(ProductDbContext context)
            {
                _context = context;
            }

            public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                _context.SaveChanges(); // Or AsyncSaveChanges
                return Task.FromResult(product);
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> Save SaveAsync
            {
                _context = _context.Products;
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
)
            })

public Task<Product> SaveAsync(Product product)
            {
                _ _context = _context;
                _context.Products.Add.Add(product);
                return _context.SaveChangesAsync);
)
)

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }


public Task<Product]

public Task

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }

public Task<Product> SaveAsync(Product product)
            {
                _context.Products.Add(product);
                return _context.SaveChangesAsync();
            }


public Task<Product> SaveAsync(Product product)
)
            {
                _context.Products.Add(product;
                return _;

public Task