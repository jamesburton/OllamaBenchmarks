using Microsoft.EntityFrameworkCore;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int PostId { get; set; }
    public bool IsApproved { get; set; }
}

public class CommentContext : DbContext
{
    public DbSet<Comment> Comments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("CommentDb");
    }
}

public static class CommentRepository
{
    public static int CountApproved(CommentContext context, int postId)
    {
        return context.Comments.Count(c => c.PostId == postId && c.IsApproved);
    }
}