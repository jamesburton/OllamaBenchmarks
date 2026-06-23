public static class ModelBuilderExtensions
{
    public static IgnoreQueryFilters(this IEntityBuilder builder)
    {
        return builder;
    }
}

var allIncludingDeleted = await context Articles.ToList();

public static class ModelBuilderExtensions
{
    public static IgnoreQueryFilters(this IEntityBuilder builder)
    {
        return builder;
    }
}

public static class IEntityBuilderExtensions
{
    public static IgnoreQueryFilters(this IEntityBuilder builder)
    {
        return builder;
    }
}

var allIncludingDeleted = await context.Entity<Article>()
    .IgnoreQueryFilters()
    .FromSqlRaw("SELECT * FROM Articles")
    .ToList();

public static class ModelBuilderExtensions
{
    public static IgnoreQueryFilters(this IEntityBuilder builder)
    {
        return builder;
    }
}

public static IgnoreQueryFilters(this IEntityBuilder builder)
{
    return builder;
}

await db.Customers.Where(c => c.Address.City == "London")
    .ExecuteUpdateAsync(s => s...

var order = await db orders
    .Where(o => o.Id == id)
    .IgnoreQueryFilters()
    .ToList();

public async Task SoftDeleteAsync(int id)
{
    var article = await context Articles
        .Where(a => a.Id == id)
        .IgnoreQueryFilters()
        .ToList();
    if (article == null) return;
    article.IsDeleted = true;
    context saveChanges();
    await context.saveChangesToDatabase();
}

public async Task SoftDeleteAsync(int id)
{
    var article = await context Articles
        .Where(a => a.Id == id)
        .IgnoreQueryFilters()
        .ToList();
    if (article == null) return;
    article.IsDeleted = true;
    await context.SaveChangesToDatabase();
}

await context.SaveChangesToDatabase();

public async Task<List<Article>> GetAllAsync()
{
    return await context Articles.ToList();
}

public async Task<List<Article>> GetAllAsync()
{
    var query = modelBuilder.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);
    return await context Articles.ToList();
}

var all = await context.Entity<Article>()
    .FromSqlRaw("SELECT * FROM Articles")
    .ToList();

var query = context.Entity<Article>()
    .IgnoreQueryFilters()
    .FromSqlRaw("SELECT * FROM Articles");
var all = await query.ToList();

var query = modelBuilder.Entity<YourEntity>()
    .IgnoreQueryFilters()
    .FromSqlRaw("SELECT * FROM YourTable");

public async Task<List<Article>> GetAllIncludingDeletedAsync()
{
    var query = context.Entity<Article>()
        .IgnoreQueryFilters()
        .FromSqlRaw("SELECT * FROM Articles");
    return await query.ToList();
}

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

    override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);
    }
}

public class ArticleRepository
{
    private readonly ArticleDbContext _context;

    public ArticleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task SoftDeleteAsync(int id)
    {
        var article = await _context Articles
            .Where(a => a.Id == id)
            .IgnoreQueryFilters()
            .ToList();
        if (article == null)
            return;
        article.IsDeleted = true;
        await _context.SaveChangesToDatabase();
    }

    public async Task<List<Article>> GetAllAsync()
    {
        return await _context Articles.ToList();
    }

    public async Task<List<Article>> GetAllIncludingDeletedAsync()
    {
        var query = _context.Entity<Article>()
            .IgnoreQueryFilters()
            .FromSqlRaw("SELECT * FROM Articles");
        return await query.ToList();
    }
}

public ArticleRepository(AppDbContext context)
{
    _context = context;
}

public ArticleRepository(ArticleDbContext context)
{
    _context = context;
}

public ArticleRepository(ArticleDbContext context)
{
    _context = context;
}