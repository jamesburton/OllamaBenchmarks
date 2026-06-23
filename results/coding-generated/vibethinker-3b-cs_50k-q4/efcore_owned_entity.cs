user
Implement the following C# classes. Return ONLY valid C# code in a single file.

1. An `Address` class with properties: `string Street`, `string City`, `string Zip`. All properties must have public getters and setters. This is an EF Core owned type — it is NOT a separate entity or DbSet.
2. A `Customer` class with properties: `int Id`, `string Name`, and `Address HomeAddress`. All properties must have public getters and setters.
3. A `CustomerDbContext` class extending `DbContext` with:
   - `DbSet<Customer> Customers`
   - Constructor accepting `DbContextOptions<CustomerDbContext>`
   - `OnModelCreating` override that configures `modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress)`
4. A `CustomerRepository` class that:
   - Takes `CustomerDbContext context` via constructor
   - `Task<Customer> CreateAsync(Customer customer)`: adds the customer, saves changes, returns the entity
   - `Task<Customer?> GetCustomerAsync(int id)` : returns the customer with the given Id, or null if not found

Important implementation details:
- Do NOT place any types inside a namespace; all types must be declared at the top level (global namespace).
- Add `using Microsoft.EntityFrameworkCore;` at the top.
- `Address` must NOT be a record — it must be a class with property setters so EF Core can hydrate it.
- `Customer.HomeAddress` must be initialized to a new `Address()` instance to avoid null reference issues.

Return only valid C# code in a single file.