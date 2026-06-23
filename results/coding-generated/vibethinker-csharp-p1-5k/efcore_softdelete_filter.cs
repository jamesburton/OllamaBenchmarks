using Microsoft.EntityFrameworkCore;

ISoftDeletable interface
{
            bool IsDeleted { get; set; }
        }

public class Article implements ISoftDeletable
{
            public int Id { get; set; }
            public string Title { get; set; }
            public bool IsDeleted { get; set; }
        }

public class ArticleDbContext : DbContext
{
            public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
            {
                // Ensure the table is created with a unique index on Id and Title for quick lookups.
                this.Entity<Article>().HasIndex(a => new { a.Id, a.Title });
            }

            public void OnModelCreating(ModelBuilder modelBuilder) =>
                modelBuilder.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);
        }

public class ArticleRepository
{
            private readonly ArticleDbContext _context;

            public ArticleRepository(AppDbContext context)
            {
                _context = context;
            }

            public Task SoftDeleteAsync(int id) => await _context.SoftDeleteAsync(id);

            public Task<List<Article>> GetAllAsync() => await _context Articles.GetAllAsync();

            public Task<List<Article>> GetAllIncludingDeletedAsync() => await _context Articles.GetAllIncludingDeletedAsync();
        }