using Microsoft.EntityFrameworkCore;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public class Article : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}

public class ArticleDbContext : DbContext
{
    public DbSet<Article> Articles { get; set; }

    public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
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

    public async Task<List<Article>> GetAllAsync()
    {
        return await _context.Articles.ToListAsync();
    }

    public async Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        return await _context.Articles
            .IgnoreQueryFilters()
            .ToListAsync();
    }
}