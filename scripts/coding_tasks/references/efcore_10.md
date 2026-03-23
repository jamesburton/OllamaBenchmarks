# EF Core 10 — Quick Reference

**Package:** `Microsoft.EntityFrameworkCore` v10+

## Left Join (new in EF10)

```csharp
var query = db.Orders
    .LeftJoin(
        db.Customers,
        order => order.CustomerId,
        customer => customer.Id,
        (order, customer) => new { order.Id, CustomerName = customer == null ? "N/A" : customer.Name }
    );
```

## Complex Types with JSON Column

```csharp
[ComplexType]
public class Address { public string Street { get; set; } public string City { get; set; } }

// In OnModelCreating:
builder.Entity<Customer>()
    .Property(c => c.Address)
    .HasJsonConversion();
```

## Bulk Update (no load required)

```csharp
await db.Orders
    .Where(o => o.Status == "Pending")
    .ExecuteUpdateAsync(s => s
        .SetProperty(o => o.Status, "Cancelled")
        .SetProperty(o => o.UpdatedAt, DateTime.UtcNow));
```

## Bulk Update on JSON Nested Property

```csharp
await db.Customers
    .Where(c => c.Address.City == "London")
    .ExecuteUpdateAsync(s => s
        .SetProperty(c => c.Address.City, "Manchester"));
```

## Bulk Delete

```csharp
await db.Orders.Where(o => o.IsArchived).ExecuteDeleteAsync();
```

## In-Memory for Testing

```csharp
services.AddDbContext<AppDbContext>(o =>
    o.UseInMemoryDatabase("TestDb"));
```

## Raw SQL

```csharp
var orders = await db.Orders
    .FromSqlRaw("SELECT * FROM Orders WHERE Status = {0}", "Active")
    .ToListAsync();
```

**Source:** https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew
