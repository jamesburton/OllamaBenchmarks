using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
        // Apply the global query filter
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
        // Use IgnoreQueryFilters() to bypass the global query filter, allowing us to find
        // records that the standard query filter would exclude (i.e., already deleted records).
        var article = await _context.Articles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article != null)
        {
            article.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Retrieves all articles (only active ones, due to the query filter).
    /// </summary>
    public async Task<List<Article>> GetAllAsync()
    {
        // The query filter automatically ensures only non-deleted articles are returned.
        return await _context.Articles.ToListAsync();
    }

    /// <summary>
    /// Retrieves all articles, including those that have been soft-deleted.
    /// </summary>
    public async Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        // Use IgnoreQueryFilters() to bypass the global query filter.
        return await _context.Articles
            .IgnoreQueryFilters()
            .ToListAsync();
    }
}