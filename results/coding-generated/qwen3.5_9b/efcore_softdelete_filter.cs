using Microsoft.EntityFrameworkCore;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public class Article : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsDeleted { get; set; }
}

public class ArticleDbContext : DbContext
{
    public DbSet<Article> Articles => Set<Article>();

    public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);

        // Ensure the IsDeleted property is mapped correctly if using default conventions, 
        // though explicit configuration isn't strictly necessary for this example.
        base.OnModelCreating(modelBuilder);
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
        var article = _context.Articles.IgnoreQueryFilters().Find(id);

        if (article == null)
        {
            return Task.CompletedTask; // Does nothing if not found
        }

        article.IsDeleted = true;
        return _context.SaveChangesAsync();
    }

    public Task<List<Article>> GetAllAsync()
    {
        // The query filter automatically handles excluding deleted articles here.
        var articles = _context.Articles.ToListAsync();
        return articles;
    }

    public Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        // Ignore the global query filter to include soft-deleted records.
        var articles = _context.Articles.IgnoreQueryFilters().ToListAsync();
        return articles;
    }
}