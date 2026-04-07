[ComplexType]
public class Address { ... }

Then in OnModelCreating:

builder.Entity<Customer>()
    .Property(c => c.Address)
    .HasJsonConversion();

So in this case, the Customer has a property called Address of type Address. So in our problem, the Customer has a property called HomeAddress. So we need to adjust that.

So putting [ComplexType] on the Address class, then in CustomerDbContext's OnModelCreating:

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.Entity<Customer>()
        .OwnsOne(c => c.HomeAddress)
        .HasJsonConversion(); // Or is it Property instead of OwnsOne?
}

Wait, no. Because if we use OwnsOne, that's for owned entity types. Wait, EF Core has owned types (complex types) which can be stored as JSON columns. So perhaps the correct configuration here is:

In the Customer entity, the HomeAddress property is a complex type (owned), and stored as JSON. So in the OnModelCreating, we need to configure that property as a JSON conversion.

Wait, the API reference example uses HasJsonConversion() when using [ComplexType]. So perhaps in the CustomerDbContext, when configuring the Customer's HomeAddress property, we need to call:

modelBuilder.Entity<Customer>()
    .Property(c => c.HomeAddress)
    .HasJsonConversion();

But then, how does EF Core know that Address is a complex type? Because the [ComplexType] attribute is applied to the Address class.

Wait, but in EF Core, the [ComplexType] attribute is equivalent to configuring the class as a complex type, which can then be used as a property in an entity. So if Address has [ComplexType], then Customer.HomeAddress can be configured with HasJsonConversion.

But in the problem's example code:

[ComplexType]
public class Address { ... }

// In OnModelCreating:
builder.Entity<Customer>()
    .Property(c => c.Address)
    .HasJsonConversion();

So in our case, Customer.HomeAddress is the property. So in OnModelCreating:

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.Entity<Customer>()
        .Property(c => c.HomeAddress)
        .HasJsonConversion();
}

But then, the Address class has [ComplexType], which tells EF Core that it's a complex type. So this should work.

Wait, but in EF Core, when you configure a property to have JSON conversion, it will store that complex type as JSON in the database. So that's the setup we need.

So the Address class has [ComplexType], and in OnModelCreating, we configure Customer.HomeAddress to use JSON conversion.

But wait, the user's problem says that Address must be an owned type (not a separate entity or DbSet). So using [ComplexType] on Address and then configuring its property in Customer as JSON conversion would meet that requirement.

So putting it all together:

Address class:

[ComplexType]
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Zip { get; set; }
}

Customer class:

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HomeAddress { get; set; } = new();
}

CustomerDbContext:

public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>()
            .Property(c => c.HomeAddress)
            .HasJsonConversion();
    }
}

Wait, but in the API reference example, the [ComplexType] is applied to Address, and then Property().HasJsonConversion() is called. So that should be correct.

But wait, in EF Core, if you have a complex type, you can either have it mapped as a complex type (with all properties mapped as columns in the same table, or as a JSON column). The HasJsonConversion() tells EF Core to store the complex type as JSON.

So in this case, the problem requires that Address is an owned type, not a separate entity. So this setup works.

Now, the Customer's HomeAddress property must be initialized to a new instance. So in the Customer class, the constructor initializes HomeAddress = new Address();.

Now, moving to the CustomerRepository:

public class CustomerRepository
{
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }
}

Wait, but for GetByIdAsync, FindAsync uses the primary key. Customer.Id is the primary key. So that's correct.

Now, putting all together in a single C# file, without any namespaces.

Also, the using Microsoft.EntityFrameworkCore; is required.

Wait, but the Address class has [ComplexType], which requires EF Core's [ComplexType] attribute. So we need to include using Microsoft.EntityFrameworkCore.Annotations; ?

Wait, in EF Core, the [ComplexType] attribute is in the Microsoft.EntityFrameworkCore.Annotations namespace. So the code should include that using statement. But the problem says to add using Microsoft.EntityFrameworkCore; at the top. Wait, the user's instruction says:

Add `using Microsoft.EntityFrameworkCore;` at the top.

But the [ComplexType] attribute is in the same namespace. Wait, checking:

No, in EF Core, the [ComplexType] attribute is in Microsoft.EntityFrameworkCore.Annotations namespace. So if we don't include that, the code won't compile.

Wait, but in the problem's API reference code:

[ComplexType]
public class Address { public string Street { get; set; } public string City { get; set; } }

The [ComplexType] attribute would require that the class either has using Microsoft.EntityFrameworkCore.Annotations; or fully qualified the attribute.

So in the generated code, the Address class needs to have [ComplexType], but since the user's code requires only using Microsoft.EntityFrameworkCore; (from the instruction), then we need to add using Annotations; ?

Wait, the user's instruction says:

Add `using Microsoft.EntityFrameworkCore;` at the top.

But the problem's code example uses [ComplexType] which requires the Annotations namespace. So the code as written would not compile unless either:

- The Address class uses the fully qualified attribute [Microsoft.EntityFrameworkCore.Annotations.ComplexTypeAttribute], or

- Add using Microsoft.EntityFrameworkCore.Annotations; to the usings.

But the user's instruction says to add only using Microsoft.EntityFrameworkCore;.

Hmm, this is a problem. Because in EF Core, the [ComplexType] attribute is in the Annotations namespace. So the code provided by the user in their API reference example would require that using directive.

Wait, but in EF Core 10, is the [ComplexType] attribute now in a different namespace? Or perhaps the user made a mistake.

Wait, checking EF Core documentation: 

The ComplexTypeAttribute is in the Microsoft.EntityFrameworkCore.Annotations namespace.

So the code example provided in the problem's API reference would require using Microsoft.EntityFrameworkCore.Annotations; to use the ComplexType attribute.

But the problem's generated code must not have any namespaces, and the user's instruction says to add using Microsoft.EntityFrameworkCore;.

So how to resolve this?

The user's generated code must compile. Therefore, the Address class needs to have the [ComplexType] attribute, but without requiring additional usings. Therefore, the attribute must be declared with a fully qualified name, or the using directive must include the Annotations namespace.

But the user's instruction says to add only using Microsoft.EntityFrameworkCore;.

Wait, but the problem's API reference code example shows:

[ComplexType]
public class Address { ... }

Which implies that the user has the appropriate using directives. Therefore, in our generated code, we need to include using Microsoft.EntityFrameworkCore.Annotations; otherwise, the code won't compile.

But the user's instruction says to add using Microsoft.EntityFrameworkCore; and no other.

This is a conflict. How to resolve?

Wait, perhaps in EF Core 10, the ComplexTypeAttribute has been moved to the main namespace. Let me check.

Looking up Microsoft.EntityFrameworkCore.ComplexTypeAttribute: according to docs, it's in the Microsoft.EntityFrameworkCore namespace, so the using statement for Microsoft.EntityFrameworkCore; would include that.

Wait, here's the documentation: 

https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.complextypeattribute?view=efcore-10.0

The ComplexTypeAttribute is in the Microsoft.EntityFrameworkCore namespace. So the using Microsoft.EntityFrameworkCore; would suffice.

Ah! So the [ComplexType] attribute is part of the Microsoft.EntityFrameworkCore namespace. So the code example in EF Core would compile if the using directive includes that namespace.

Therefore, in our code, the Address class can have the [ComplexType] attribute without any problem.

So the code:

[ComplexType]
public class Address
{
    // properties
}

And with using Microsoft.EntityFrameworkCore; at the top, that's correct.

Great, no problem.

Now, putting all together:

The code:

using Microsoft.EntityFrameworkCore;

[ComplexType]
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Zip { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HomeAddress { get; set; } = new();
}

public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>()
            .OwnsOne(c => c.HomeAddress);
    }
}

public class CustomerRepository
{
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }
}

Wait, but wait in the OnModelCreating: the problem's API reference example uses HasJsonConversion when using ComplexType. But in the current problem, the user hasn't specified whether the Address should be stored as JSON or as owned type with table splitting. However, according to the problem's first requirement, Address must be an owned type. So configuring OwnsOne(c => c.HomeAddress) in the OnModelCreating would suffice. However, in the API reference example, the JSON conversion is used. But in our case, the problem says that Address is an owned type. So perhaps the OwnsOne is the correct configuration.

Wait, the problem says that Address must be an owned type (not a separate entity or DbSet). So using OwnsOne is correct. But in EF Core, when you configure OwnsOne, the properties of the owned type are mapped to the same table (unless configured otherwise). So the Address properties would be stored in the same Customer table, with column names like HomeAddress_Street, etc.

But the API reference example uses HasJsonConversion(). Which approach should be used here?

The problem says that the Address is an owned type. So perhaps the [ComplexType] and OwnsOne configuration is the way to go. However, the