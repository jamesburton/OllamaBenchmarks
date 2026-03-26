using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

public class Product
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public string Category { get; set; }

    public DateTime LastModified { get; set; }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configure the database connection string here if needed.
        // For example: optionsBuilder.UseSqlServer("YourConnectionString");
    }

}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        int affectedRows = 0;

        var productsToUpdate = await db.Products
            .Where(p => p.Category == category).ToListAsync();

        foreach (var product in productsToUpdate)
        {
            if (!product.LastModified.HasValue) // Check to avoid updating new entries without a LastModified value.
                continue;

            decimal originalPrice = product.Price;

            try
            {
                affectedRows++;

                product.Price *= (1 - discountPct);
                product.LastModified = DateTime.UtcNow;

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle exceptions as appropriate for your application.
                Console.WriteLine(ex.Message);
            }
        }

        return affectedRows;
    }
}