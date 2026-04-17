using Microsoft.EntityFrameworkCore;
using System;

public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BlogContext : DbContext
{
    public DbSet<BlogPost> BlogPosts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("BlogPostDb");
    }
}

public static class BlogRepository
{
    public static BlogPost Add(BlogContext context, string title, string content)
    {
        var post = new BlogPost
        {
            Title = title,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        context.BlogPosts.Add(post);
        context.SaveChanges();

        return post;
    }
}