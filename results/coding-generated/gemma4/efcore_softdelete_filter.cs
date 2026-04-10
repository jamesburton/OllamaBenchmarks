using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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

    public async Task SoftDeleteAsync(int id)
    {
        // Use IgnoreQueryFilters() to find the record regardless of the active filter
        var article = await _context.Articles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article != null && !article.IsDeleted)
        {
            article.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Article>> GetAllAsync()
    {
        // The query filter automatically handles filtering for IsDeleted == false
        return await _context.Articles.ToListAsync();
    }

    public async Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        // Use IgnoreQueryFilters() to bypass the soft delete filter
        return await _context.Articles
            .IgnoreQueryFilters()
            .ToListAsync();
    }
}