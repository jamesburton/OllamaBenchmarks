using Microsoft.EntityFrameworkCore;

public class File
{
    public int Id { get; set; }
    public string Path { get; set; }
    public long SizeBytes { get; set; }
    public string MimeType { get; set; }
}

public class FileContext : DbContext
{
    public DbSet<File> Files { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("FileDatabase");
    }
}

public static class FileRepository
{
    public static long GetTotalSize(FileContext context, string mimeType)
    {
        return context.Files
            .Where(f => f.MimeType == mimeType)
            .Sum(f => f.SizeBytes);
    }
}