using Microsoft.EntityFrameworkCore;

public class Bookmark
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
}

public class BookmarkContext : DbContext
{
    public DbSet<Bookmark> Bookmarks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("BookmarkDb");
    }
}

public static class BookmarkRepository
{
    public static bool Exists(BookmarkContext context, string userId, string url)
    {
        return context.Bookmarks.Any(b => 
            b.UserId.ToLower() == userId.ToLower() && 
            b.Url.ToLower() == url.ToLower());
    }
}