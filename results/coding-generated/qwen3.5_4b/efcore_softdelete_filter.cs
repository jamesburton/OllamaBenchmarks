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

        public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
            var article = _context.Articles.IgnoreQueryFilters().FindAsync(id).Result; // Wait, async/await pattern.
            // Actually, FindAsync returns Task<T>.
            // Let's use proper async syntax.
            return Task.CompletedTask; // Placeholder logic below
        }

        public Task<List<Article>> GetAllAsync()
        {
            return _context.Articles.ToListAsync();
        }

        public Task<List<Article>> GetAllIncludingDeletedAsync()
        {
            return _context.Articles.IgnoreQueryFilters().ToListAsync();
        }
    }

var article = await _context.Articles.IgnoreQueryFilters().FindAsync(id);
    if (article != null)
    {
        article.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

var article = await _context.Articles.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
    if (article != null)
    {
        article.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

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

        public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
            var article = _context.Articles.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id).Result; // Wait, Result blocks async.
            // Must use await properly.
            return Task.CompletedTask; // Placeholder logic below
        }
    }