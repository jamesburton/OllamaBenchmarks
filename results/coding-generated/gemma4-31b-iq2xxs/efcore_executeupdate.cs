*   API Reference provided: EF Core 10.
    *   Goal: Create a single C# file.
    *   Requirements:
        1.  `Product` entity class (Id, Name, Price, Category, LastModified).
        2.  `ProductDbContext` (inheriting `DbContext`, `DbSet<Product> Products`).
        3.  `ProductOperations` class with `ApplyDiscount` method.
        1.  `ApplyDiscount` logic:
            *   Input: `ProductDbContext db`, `string category`, `decimal discountPct`.
            *   Logic: Update Price and LastModified for products in the specified category.
            *   Constraint: Since InMemory provider doesn't support `ExecuteUpdateAsync`, use a manual loop (query, update, `SaveChangesAsync`).
            *   Return: Number of rows affected.
    *   Constraints:
es’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’s’