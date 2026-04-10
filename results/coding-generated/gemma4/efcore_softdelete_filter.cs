using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

// 1. ISoftDeletable interface
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

// 2. Article class implementing ISoftDeletable
public class Article : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsDeleted { get; set; }
}

// 3. ArticleDbContext class
public class ArticleDbContext : DbContext
{
    public DbSet<Article> Articles { get; set; }

    public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply query filter for soft deletion
        modelBuilder.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);
    }
}

// 4. ArticleRepository class
public class ArticleRepository
{
    private readonly ArticleDbContext _context;

    public ArticleRepository(ArticleDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Soft deletes an article by setting IsDeleted to true.
    /// Uses IgnoreQueryFilters() to find the record regardless of its current IsDeleted state.
    /// </summary>
    public async Task SoftDeleteAsync(int id)
    {
        // Use IgnoreQueryFilters() to find the record even if it's already marked deleted
        var article = await _context.Articles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article != null)
        {
            if (article.IsDeleted == false)
            {
                article.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Retrieves all active articles (respects the query filter).
    /// </summary>
    public async Task<List<Article>> GetAllAsync()
    {
        // The query filter automatically handles filtering out deleted items
        return await _context.Articles.ToListAsync();
    }

    /// <summary>
    /// Retrieves all articles, including soft-deleted ones.
    /// </summary>
    public async Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        // Use IgnoreQueryFilters() to bypass the default filter
        return await _context.Articles
            .IgnoreQueryFilters()
            .ToListAsync();
    }
}