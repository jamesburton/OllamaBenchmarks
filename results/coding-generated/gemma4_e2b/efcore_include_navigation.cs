using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

// 1. Comment Class
public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int PostId { get; set; }

    // Navigation property
    public Post? Post { get; set; }
}

// 2. Post Class
public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int BlogId { get; set; }

    // Navigation properties
    public Blog? Blog { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

// 3. Blog Class
public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; }

    // Navigation property
    public List<Post> Posts { get; set; } = new();
}

// 4. BlogDbContext Class
public class BlogDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Blog to Post relationship (One-to-Many)
        modelBuilder.Entity<Blog>()
            .HasMany(b => b.Posts)
            .WithOne(p => p.Blog)
            .HasForeignKey(p => p.BlogId);

        // Configure Post to Comment relationship (One-to-Many)
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId);
    }
}

// 5. BlogRepository Class
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