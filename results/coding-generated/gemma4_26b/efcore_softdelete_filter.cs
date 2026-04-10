using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public class Article : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsDeleted { get; set; }
    public ISoftDeletable IsDeleted { get; set; } // Note: Property name matches interface requirement
}

// Re-defining Article to strictly follow the prompt's property list and interface requirements
public class ArticleEntity : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsDeleted { get; set; }
}

public class ArticleDbContext : DbContext
{
    public DbSet<ArticleEntity> Articles { get; set; }

    public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticleEntity>().HasQueryFilter(a => !a.IsDeleted);
    }
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
        var article = await _context.Articles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article != null)
        {
            article.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<ArticleEntity>> GetAllAsync()
    {
        return await _context.Articles.ToListAsync();
    }

    public async Task<List<ArticleEntity>> GetAllIncludingDeletedAsync()
    {
        return await _context.Articles
            .IgnoreQueryFilters()
            .ToListAsync();
    }
}