using System;
using Microsoft.EntityFrameworkCore;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsDeleted { get; set; }
}

public class ArticleDbContext : DbContext
{
    public ArticleDbContext(DbOptions options) : base(options) { }
    public DbSet<Article> Articles { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);
    }
}

public class ArticleRepository
{
    private readonly ArticleDbContext _context;

    public ArticleRepository(ArticleDbContext options) : _context(options) { }

    public async Task SoftDeleteAsync(int id)
    {
        var article = _context.Articles.Find(id);
        if (article != null)
        {
            article.IsDeleted = true;
            _context.SaveChanges();
        }
    }

    public async Task<List<Article>> GetAllAsync() => await _context.Articles.ToListAsync();
    public async Task<List<Article>> GetAllIncludingDeletedAsync() => await _context.Articles.ToListAsync();
}