using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options) { }
}

public static class ProductOperations
{
    public static async Task<int> ApplyDiscount(
        ProductDbContext db,
        string category,
        decimal discountPct)
    {
        var affected = 0;
        var nowUtc = DateTime.UtcNow;

        var products = await db.Products
            .Where(p => p.Category == category)
            .ToListAsync();

        foreach (var product in products)
        {
            product.Price *= (1 - discountPct);
            product.LastModified = nowUtc;
            affected++;
        }

        await db.SaveChangesAsync();
        return affected;
    }
}

public static class DbContextExtensions
{
    public static async Task<int> ExecuteUpdateAsync<TEntity>(
        this ProductDbContext db,
        Func<IQueryable<TEntity>, IQueryable> filter,
        Action<UpdateBuilder<TEntity>> updateAction) where TEntity : class
    {
        throw new InvalidOperationException(
            "The ExecuteUpdateAsync extension is not supported in the InMemory provider.");
    }

    public class UpdateBuilder<TEntity> where TEntity : class
    {
        public UpdateBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, Func<TProperty, TProperty> valueModifier)
        {
            return this;
        }
    }
}