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

                if (article != null)
                {
                    article.IsDeleted = true;
                    _context.SaveChanges();
                }

                return Task.CompletedTask;
            }

            public Task<List<Article>> GetAllAsync()
            {
                // Query filter applies automatically here
                return _context.Articles.ToListAsync();
            }
            }

            public Task<List<Article>> GetAllAllIncludingDeletedAsync()
            {
                // Ignore
                return _context.Art
                    .Ignore
                    .Ignore
                    .IgnoreQueryFilters()
                    .ToListAsync();
            }
        }