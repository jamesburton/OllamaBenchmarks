using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int AuthorId { get; set; }
    public Author Author { get; set; }
}

public class LibraryContext : DbContext
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("LibraryDatabase");
    }
}

public static class LibraryRepository
{
    public static List<Book> GetBooksByAuthor(LibraryContext context, int authorId)
    {
        return context.Books
            .Include(b => b.Author)
            .Where(b => b.AuthorId == authorId)
            .OrderBy(b => b.Title)
            .ToList();
    }
}