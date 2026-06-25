using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public class Article : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsDeleted { get; set; }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}

public class ArticleDbContext : DbContext, ISoftDeletable
{
    public DbSet<Article> Articles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);
    }

    public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options) {}

}

public class ArticleRepository
{
    private readonly ArticleDbContext _context;

    public ArticleRepository(ArticleDbContext context)
    {
        _context = context;
    }

    public async Task SoftDeleteAsync(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article != null)
            article.IsDeleted = true;
        else
            throw new ArgumentException($"Article with Id {id} not found.");
    }

    public async Task<List<Article>> GetAllAsync()
    {
        return await _context.Articles.Where(a => !a.IsDeleted).ToListAsync();
    }

    public async Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        return await _context.Articles.IgnoreQueryFilters().ToListAsync();
    }
}