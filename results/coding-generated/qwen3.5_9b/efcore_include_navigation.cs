using Microsoft.EntityFrameworkCore;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public int PostId { get; set; }
    public Post? Post { get; set; }
}

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public List<Post> Posts { get; set; } = new();
}

public class BlogDbContext : DbContext
{
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();

    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>()
            .HasMany(b => b.Posts)
            .WithOne(p => p.Blog!)
            .HasForeignKey(p => p.BlogId);

        modelBuilder.Entity<Post>()
            .HasMany(c => c.Comments)
            .WithOne(com => com.Post!)
            .HasForeignKey(com => com.PostId);
    }
}

public class BlogRepository
{
    private readonly BlogDbContext _context;

    public BlogRepository(BlogDbContext context)
    {
        _context = context;
    }

    public Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
    {
        return _context.Blogs
            .Include(b => b.Posts)
                .ThenInclude(p => p.Comments)
            .FirstOrDefaultAsync(b => b.Id == blogId);
    }
}