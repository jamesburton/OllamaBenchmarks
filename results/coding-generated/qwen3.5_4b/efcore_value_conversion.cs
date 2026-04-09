public record Money(decimal Amount, string Currency);

public class Product {
            public int Id { get; set; }
            public string Name { get; set; }
            public Money Price { get; set; } = new Money(0, "USD");
        }

private static Money ParseMoney(string s) {
            var parts = s.Split(':');
            if (parts.Length != 2) throw new ArgumentException("Invalid format");
            return new Money(decimal.Parse(parts[0]), parts[1]);
        }

protected override void OnModelCreating(ModelBuilder modelBuilder) {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<Product>()
                    .Property(p => p.Price)
                    .HasConversion(m => $"{m.Amount}:{m.Currency}", s => ParseMoney(s));
            }