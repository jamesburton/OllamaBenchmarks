public async Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
{
    var blog = await context.Blogs.Find(b => b.Id == blogId);
    if (blog == null) return null;
    return blog Include(b => b.Posts).ThenInclude(p => p.Comments).First();
}

using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
}

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<Post> Posts { get; set; } = new();
}

public class BlogDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

    override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<Post>(p => p.BlogId)
            .HasMany(b => b.Posts);

        modelBuilder.Entity<Comment>(c => c.PostId)
            .HasMany(p => p.Comments);
    }
}

public class BlogRepository
{
    public BlogRepository(BlogDbContext context) : this(context) { }

    public async Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
    {
        var blog = await context.Blogs.Find(b => b.Id == blogId);
        if (blog == null) return null;
        return blog
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)
            .First();
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public async System.Threading.Tasks.Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
{
    ...
}

public async System.Threading.Tasks.Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
{
    var blog = await context.Blogs.Find(b => b.Id == blogId);
    if (blog == null) return null;
    return blog
        .Include(b => b.Posts)
        .ThenInclude(p => p.Comments)
        .First();
}

public async System.Threading.Tasks.Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
{
    ...
}

using Microsoft.EntityFrameworkCore;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
}

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<Post> Posts { get; set; } = new();
}

public class BlogDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

    override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure blog-to-post relationship
        modelBuilder.Entity<Post>(p => p.BlogId)
            .HasMany(b => b.Posts);

        // Configure post-to-comment relationship
        modelBuilder.Entity<Comment>(c => c.PostId)
            .HasMany(p => p.Comments);
    }
}

public class BlogRepository
{
    public BlogRepository(BlogDbContext context) : this(context) { }

    public async System.Threading.Tasks.Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
    {
        var blog = await context.Blogs.Find(b => b.Id == blogId);
        if (blog == null) return null;
        return blog
            .Include(b => b.Posts)
            .ThenInclude(p => p.Comments)
            .First();
    }
}