using Microsoft.EntityFrameworkCore;

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<Post> Posts { get; set; } = new();
}

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
}

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

public class BlogRepository
{
    public async Task<Blog?> GetWithPostsAndCommentsAsync(BlogDbContext context, int blogId)
    {
        var blog = await context.Blogs.FindAsync(blogId);
        if (blog == null)
        {
            return null;
        }

        blog.Posts = await context.Posts.Where(p => p.BlogId == blogId).ToListAsync();
        blog.Comments = await context.Comments.Where(c => c.PostId == blogId).ToListAsync();

        return blog;
    }
}