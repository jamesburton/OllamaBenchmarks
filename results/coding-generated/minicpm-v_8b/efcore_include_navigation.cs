using Microsoft.EntityFrameworkCore;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int PostId { get; set; }
    public virtual Post Post { get; set; } // Navigation property with nullable type: Blog?
}

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int BlogId { get; set; }
    public virtual Blog Blog { get; set; } // Navigation property with nullable type: Blog?
    public virtual List<Comment> Comments { get; set; } // Collection initialized to new()
}

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; }
    public virtual List<Post> Posts { get; set; } // Collection initialized to new()
}

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
        modelBuilder.Entity<Post>().HasOne(b => b.Blog).WithMany().HasForeignKey(b => b.BlogId);
        modelBuilder.Entity<Comment>().HasOne(c => c.Post).WithMany().HasForeignKey(c => c.PostId);
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
            .Include(b => b.Posts).ThenInclude(p => p.Comments)
            .FirstOrDefaultAsync(b => b.Id == blogId);
    }
}