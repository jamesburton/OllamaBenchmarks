using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply the query filter to ensure that by default, only non-deleted items are retrieved
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
    /// Soft deletes an article by setting its IsDeleted flag to true.
    /// </summary>
    public async Task SoftDeleteAsync(int id)
    {
        // Find the article, ignoring the default query filter to find soft-deleted items if necessary
        var article = await _context.Articles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article != null)
        {
            article.IsDeleted = true;
            // SaveChanges will apply the change
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Retrieves all articles where IsDeleted is false (default behavior enforced by query filter).
    /// </summary>
    public async Task<List<Article>> GetAllAsync()
    {
        // The query filter in OnModelCreating ensures this only returns non-deleted items
        return await _context.Articles.ToListAsync();
    }

    /// <summary>
    /// Retrieves all articles, including those that have been soft-deleted.
    /// </summary>
    public async Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        // Use IgnoreQueryFilters() to bypass the default filter
        return await _context.Articles
            .IgnoreQueryFilters()
            .ToListAsync();
    }
}