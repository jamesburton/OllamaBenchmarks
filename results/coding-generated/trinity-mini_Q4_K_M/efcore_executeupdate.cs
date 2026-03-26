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

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {

    }
}

public static class ProductOperations
{
    public async static Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var products = await db.Products.Where(p => p.Category == category).ToListAsync();
        int updatedCount = 0;
        foreach (var product in products)
        {
            product.Price *= (1 - discountPct);
            product.LastModified = DateTime.UtcNow;
            updatedCount++;
        }
        await db.SaveChangesAsync();
        return updatedCount;
    }
}

The code defines:
- A `Product` entity class with public properties for Id, Name, Price, Category, and LastModified.
- A `ProductDbContext` class that inherits from DbContext and has a DbSet<Product> property. It includes a constructor that accepts DbContextOptions<ProductDbContext>.
- A static class `ProductOperations` containing an async method ApplyDiscount which takes a ProductDbContext instance, category string, and discountPct decimal. The method:
  1. Queries all products matching the given category.
  2. Iterates through each product, updating Price (multiplying by 1 - discountPct) and setting LastModified to DateTime.UtcNow.
  3. Counts how many products were updated.
  4. Saves changes asynchronously using SaveChangesAsync.
  5. Returns the count of updated products.

The solution avoids using ExecuteUpdateAsync as per the requirement for manual updates with SaveChangesAsync, making it testable with the InMemory provider. The code is valid C# and follows all specified constraints.

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

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {

    }
}

public static class ProductOperations
{
    public async static Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var products = await db.Products.Where(p => p.Category == category).ToListAsync();
        int updatedCount = 0;
        foreach (var product in products)
        {
            product.Price *= (1 - discountPct);
            product.LastModified = DateTime.UtcNow;
            updatedCount++;
        }
        await db.SaveChangesAsync();
        return updatedCount;
    }
}

The code defines:
- A `Product` entity class with public properties for Id, Name, Price, Category, and LastModified.
- A `ProductDbContext` class that inherits from DbContext and has a DbSet<Product> property. It includes a constructor that accepts DbContextOptions<ProductDbContext>.
- A static class `ProductOperations` containing an async method ApplyDiscount which takes a ProductDbContext instance, category string, and discountPct decimal. The method:
  1. Queries all products matching the given category.
  2. Iterates through each product, updating Price (multiplying by 1 - discountPct) and setting LastModified to DateTime.UtcNow.
  3. Counts how many products were updated.
  4. Saves changes asynchronously using SaveChangesAsync.
  5. Returns the count of updated products.

The solution avoids using ExecuteUpdateAsync as per the requirement for manual updates with SaveChangesAsync, making it testable with the InMemory provider. The code is valid C# and follows all specified constraints.

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

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {

    }
}

public static class ProductOperations
{
    public async static Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var products = await db.Products.Where(p => p.Category == category).ToListAsync();
        int updatedCount = 0;
        foreach (var product in products)
        {
            product.Price *= (1 - discountPct);
            product.LastModified = DateTime.UtcNow;
            updatedCount++;
        }
        await db.SaveChangesAsync();
        return updatedCount;
    }
}

The code defines:
- A `Product` entity class with public properties for Id, Name, Price, Category, and LastModified.
- A `ProductDbContext` class that inherits from DbContext and has a DbSet<Product> property. It includes a constructor that accepts DbContextOptions<ProductDbContext>.
- A static class `ProductOperations` containing an async method ApplyDiscount which takes a ProductDbContext instance, category string, and discountPct decimal. The method:
  1. Queries all products matching the given category.
  2. Iterates through each product, updating Price (multiplying by 1 - discountPct) and setting LastModified to DateTime.UtcNow.
  3. Counts how many products were updated.
  4. Saves changes asynchronously using SaveChangesAsync.
  5. Returns the count of updated products.

The solution avoids using ExecuteUpdateAsync as per the requirement for manual updates with SaveChangesAsync, making it testable with the InMemory provider. The code is valid C# and follows all specified constraints.

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

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {

    }
}

public static class ProductOperations
{
    public async static Task<int> ApplyDiscount(ProductDbContext db, string category, decimal discountPct)
    {
        var products = await db.Products.Where(p => p.Category == category).ToListAsync();
        int updatedCount = 0;
        foreach (var product in products)
        {
            product.Price *= (1 - discountPct);
            product.LastModified = DateTime.UtcNow;
            updatedCount++;
        }
        await db.SaveChangesAsync();
        return updatedCount;
    }
}

The code defines:
- A `Product` entity class with public properties for Id, Name, Price, Category, and LastModified.
- A `ProductDbContext` class that inherits from DbContext and has a DbSet<Product> property. It includes a constructor that accepts DbContextOptions<ProductDbContext>.
- A static class `ProductOperations` containing an async method ApplyDiscount which takes a ProductDbContext instance, category string, and discountPct decimal. The method:
  1. Queries all products matching the given category.
  2. Iterates through each product, updating Price (multiplying by 1 - discountPct) and setting LastModified to DateTime.UtcNow.
  3. Counts how many products were updated.
  4. Saves changes asynchronously using SaveChangesAsync.
  5. Returns the count of updated products.

The solution avoids using ExecuteUpdateAsync as per the requirement for manual updates with SaveChangesAsync, making it testable with the InMemory provider. The code is valid C# and follows all specified constraints.