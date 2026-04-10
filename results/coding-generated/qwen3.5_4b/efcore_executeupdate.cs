public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string Category { get; set; }
            public DateTime LastModified { get; set; }
        }

public class ProductDbContext : DbContext
        {
            public DbSet<Product> Products { get; set; }

            public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
        }

public static class ProductOperations
        {
            public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
            {
                // Logic: Query matching products, update manually, save, return count.
                var products = await db.Products.ToListAsync();
                int updatedCount = 0;

                foreach (var product in products)
                {
                    if (product.Category == category)
                    {
                        product.Price = product.Price * (1 - discountPct);
                        product.LastModified = DateTime.UtcNow;
                        updatedCount++;
                    }
                }

                await db.SaveChangesAsync();
                return updatedCount;
            }
        }

namespace MyProject {
        public class Product ...
    }

public class Product ...

using System;
using Microsoft.EntityFrameworkCore;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var products = await db.Products.ToListAsync();
        int updatedCount = 0;

        foreach (var product in products)
        {
            if (product.Category == category)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
                updatedCount++;
            }
        }

        await db.SaveChangesAsync();
        return updatedCount;
    }
}

using ...
public class ...

namespace ...
{
    public class ...
}

using System;
using Microsoft.EntityFrameworkCore;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var products = await db.Products.ToListAsync();
        int updatedCount = 0;

        foreach (var product in products)
        {
            if (product.Category == category)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
                updatedCount++;
            }
        }

        await db.SaveChangesAsync();
        return updatedCount;
    }
}

... 
using System;
using Microsoft.EntityFrameworkCore;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var products = await db.Products.ToListAsync();
        int updatedCount = 0;

        foreach (var product in products)
        {
            if (product.Category == category)
            {
                product.Price = product.Price * (1 - discountPct);
                product.LastModified = DateTime.UtcNow;
                updatedCount++;
            }
        }

        await db.SaveChangesAsync();
        return updatedCount;
    }
}