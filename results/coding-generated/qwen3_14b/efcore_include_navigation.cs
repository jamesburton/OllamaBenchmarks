using Microsoft.EntityFrameworkCore;

// Comment class
public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public int PostId { get; set; }
    public Post? Post { get; set; } = null;
}

// Post class
public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public int BlogId { get; set; }
    public Blog? Blog { get; set; } = null;
    public List<Comment> Comments { get; } = new();
}

// Blog class
public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public List<Post> Posts { get; } = new();
}

// BlogDbContext class
public class BlogDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;

    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

// BlogRepository class
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