using Microsoft.EntityFrameworkCore;

public interface ISoftDeletable { bool IsDeleted { get; set; } }

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
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);
    }
}

public class ArticleRepository
{
    public ArticleDbContext Context { get; }
    public ArticleRepository(ArticleDbContext context) => Context = context;

    public async Task SoftDeleteAsync(int id)
    {
        var article = await Context.Articles.IgnoreQueryFilters().FindAsync(a => a.Id == id);
        if (article != null)
        {
            article.Value.IsDeleted = true;
            await Context.Articles.IgnoreQueryFilters().AddAllAsync();
        }
    }

    public async Task<List<Article>> GetAllAsync()
    {
        return await Context.Articles.Where(a => !a.IsDeleted).ToListAsync();
    }

    public async Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        return await Context.Articles.IgnoreQueryFilters().ToListAsync();
    }
}