using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

public class Note
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public bool IsDeleted { get; set; }
}

public class NoteContext : DbContext
{
    public DbSet<Note> Notes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("NoteDb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>().HasQueryFilter(n => !n.IsDeleted);
    }
}

public static class NoteRepository
{
    public static List<Note> GetAll(NoteContext context)
    {
        return context.Notes.ToList();
    }
}