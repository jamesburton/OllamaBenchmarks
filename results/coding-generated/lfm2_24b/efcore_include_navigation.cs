using Microsoft.EntityFrameworkCore;

// Entity classes
public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public int PostId { get; set; }
    public Post? Post { get; set; }
    public List<Comment>? Comments { get; set; } = new List<Comment>();
}

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
    public List<Comment>? Comments { get; set; } = new List<Comment>();
}

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public List<Post>? Posts { get; set; } = new List<Post>();
}

// DbContext
public class BlogDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;

    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Blog has many Posts (foreign key BlogId)
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Blog)
            .WithMany(b => b.Posts)
            .HasForeignKey(p => p.BlogId);

        // Post has many Comments (foreign key PostId)
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId);
    }
}

// Repository
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
            .Where(b => b.Id == blogId)
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)
            .FirstOrDefaultAsync();
    }
}