using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

// 1. ISoftDeletable interface
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

// 2. Article class
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

        // Apply the query filter: only retrieve articles where IsDeleted is false by default
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
    /// Soft deletes an article by setting IsDeleted = true.
    /// Uses IgnoreQueryFilters() to find the record even if it was already deleted.
    /// </summary>
    public async Task SoftDeleteAsync(int id)
    {
        // Use IgnoreQueryFilters() to find the record regardless of the active query filter
        var article = await _context.Articles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article != null && !article.IsDeleted)
        {
            article.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
        // If article is null or already deleted, do nothing.
    }

    /// <summary>
    /// Retrieves all active articles (respects the query filter).
    /// </summary>
    public async Task<List<Article>> GetAllAsync()
    {
        // The query filter handles the IsDeleted == false condition automatically.
        return await _context.Articles.ToListAsync();
    }

    /// <summary>
    /// Retrieves all articles, including soft-deleted ones.
    /// </summary>
    public async Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        // Use IgnoreQueryFilters() to bypass the default filtering logic.
        return await _context.Articles
            .IgnoreQueryFilters()
            .ToListAsync();
    }
}