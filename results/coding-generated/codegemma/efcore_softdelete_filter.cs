using Microsoft.EntityFrameworkCore;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public class Article : ISoftDeletable
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsDeleted { get; set; }
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

    public async Task SoftDeleteAsync(int id)
    {
        var article = await context.Articles.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
        if (article != null)
        {
            article.IsDeleted = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Article>> GetAllAsync()
    {
        return await context.Articles.Where(a => !a.IsDeleted).ToListAsync();
    }

    public async Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        return await context.Articles.IgnoreQueryFilters().ToListAsync();
    }
}