using Microsoft.EntityFrameworkCore;

// Comment entity
public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; } = default!;
    public int PostId { get; set; }
    public Post? Post { get; set; }
}

// Post entity
public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = default!;
    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

// Blog entity
public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public List<Post> Posts { get; set; } = new();
}

// DbContext
public class BlogDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

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

// Repository
public class BlogRepository
{
    private readonly BlogDbContext _context;
    public BlogRepository(BlogDbContext context) => _context = context;

    public async Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
    {
        return await _context.Blogs
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)
            .SingleOrDefaultAsync(b => b.Id == blogId);
    }
}