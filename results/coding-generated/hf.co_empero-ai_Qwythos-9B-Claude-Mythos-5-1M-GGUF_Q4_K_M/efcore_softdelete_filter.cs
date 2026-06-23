using Microsoft.EntityFrameworkCore;

public interface ISoftDeletable { bool IsDeleted { get; set; } }

public class Article : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; }
}

public class ArticleDbContext : DbContext
{
    public DbSet<Article> Articles { get; set; }

    public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);
    }
}

public class ArticleRepository
{
    private readonly ArticleDbContext context;

    public ArticleRepository(ArticleDbContext context)
    {
        this.context = context;
    }

    public Task SoftDeleteAsync(int id)
    {
        var article = await context.Articles.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
        if (article != null)
        {
            article.IsDeleted = true;
            return context.SaveChangesAsync();
        }
        return Task.CompletedTask;
    }

    public Task<List<Article>> GetAllAsync()
    {
        return context.Articles.ToListAsync();
    }

    public Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        return context.Articles.IgnoreQueryFilters().Where(a => a.IsDeleted == false).ToListAsync();
    }
}