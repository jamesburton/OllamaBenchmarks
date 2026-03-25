using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ProductDemo
{
    /// <summary>
    /// Represents a product in the catalog.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public decimal Price { get; set; }

        public string Category { get; set; } = null!;

        public DateTime LastModified { get; set; }
    }

    /// <summary>
    /// EF Core context for <see cref="Product"/>.
    /// </summary>
    public class ProductDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; } = null!;

        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Optional: map to a specific table name.
            modelBuilder.Entity<Product>().ToTable("Products");
        }
    }

    /// <summary>
    /// Bulk‑operation helpers for <see cref="Product"/>.
    /// </summary>
    public static class ProductOperations
    {
        /// <summary>
        /// Applies a discount to all products in the specified category.
        /// </summary>
        /// <param name="db">The EF Core context.</param>
        /// <param name="category">Category to filter on.</param>
        /// <param name="discountPct">Discount expressed as a fraction (e.g., 0.15 for 15%).</param>
        /// <returns>The number of rows that were updated.</returns>
        public static async Task<int> ApplyDiscountAsync(
            ProductDbContext db,
            string category,
            decimal discountPct)
        {
            // Compute the multiplier (e.g., 0.85 for a 15% discount).
            decimal multiplier = 1 - discountPct;

            return await db.Products
                .Where(p => p.Category == category)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Price, p => p.Price * multiplier)
                    .SetProperty(p => p.LastModified, DateTime.UtcNow));
        }

        // Expose a method with the exact name requested in the prompt.
        public static Task<int> ApplyDiscount(
            ProductDbContext db,
            string category,
            decimal discountPct) => ApplyDiscountAsync(db, category, discountPct);
    }
}