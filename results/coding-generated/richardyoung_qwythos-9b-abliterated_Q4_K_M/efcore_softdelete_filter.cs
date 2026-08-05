using Microsoft.EntityFrameworkCore;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public class Article : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; }
}

public class ArticleDbContext : DbContext
{
    public DbSet<Article> Articles { get; set; }

    public ArticleDbContext(DbContextOptions<ArticleDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);
    }
}

public class ArticleRepository
{
    private readonly ArticleDbContext _context;

    public ArticleRepository(ArticleDbContext context)
    {
        _context = context;
    }

    public Task SoftDeleteAsync(int id)
    {
        var article = _context.Articles
            .IgnoreQueryFilters()
            .FirstOrDefault(a => a.Id == id);

        if (article is null)
            return Task.CompletedTask;

        article.IsDeleted = true;
        return _context.SaveChangesAsync();
    }

    public Task<List<Article>> GetAllAsync()
    {
        // Query filter automatically excludes deleted rows.
        return _context.Articles.ToListAsync();
    }

    public Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        // Ignore query filters to include soft-deleted rows.
        return _context.Articles
            .IgnoreQueryFilters()
            .ToListAsync();
    }
}