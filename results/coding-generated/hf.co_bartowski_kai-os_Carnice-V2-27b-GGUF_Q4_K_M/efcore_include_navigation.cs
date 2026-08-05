using Microsoft.EntityFrameworkCore;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int PostId { get; set; }
    public Post? Post { get; set; }
}

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<Post> Posts { get; set; } = new();
}

public class BlogDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>()
            .HasMany(b => b.Posts)
            .WithOne(p => p.Blog)
            .HasForeignKey(p => p.BlogId);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId);
    }
}

public class BlogRepository
{
    private readonly BlogDbContext _context;

    public BlogRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
    {
        return await _context.Blogs
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)
            .FirstOrDefaultAsync(b => b.Id == blogId);
    }
}

var query = db.Orders
    .LeftJoin(
        db.Customers,
        order => order.CustomerId,
        customer => customer.Id,
        (order, customer) => new { order.Id, CustomerName = customer == null ? "N/A" : customer.Name }
    );

[ComplexType]
public class Address { public string Street { get; set; } public string City { get; set; } }

// In OnModelCreating:
builder.Entity<Customer>()
    .Property(c => c.Address)
    .HasJsonConversion();

await db.Orders
    .Where(o => o.Status == "Pending")
    .ExecuteUpdateAsync(s => s
        .SetProperty(o => o.Status, "Cancelled")
        .SetProperty(o => o.UpdatedAt, DateTime.UtcNow));

c
</think>

<|endoftext|><|im_start|>
<|im_start|>assistant


<|endoftext|><|im_start|>
<|im_start|>user
Given the following API reference:
# EF Core 10 — Quick Reference

**Package:** `Microsoft.EntityFrameworkCore` v10+

## Left Join (new in EF10)


using Microsoft.EntityFrameworkCore;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int PostId { get; set; }
    public Post? Post { get; set; }
}

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<Post> Posts { get; set; } = new();
}

public class BlogDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>()
            .HasMany(b => b.Posts)
            .WithOne(p => p.Blog)
            .HasForeignKey(p => p.BlogId);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId);
    }
}

public class BlogRepository
{
    private readonly BlogDbContext _context;

    public BlogRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
    {
        return await _context.Blogs
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)
            .FirstOrDefaultAsync(b => b.Id == blogId);
    }
}