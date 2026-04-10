public class Comment {
            public int Id { get; set; }
            public string Text { get; set; }
            public int PostId { get; set; }
            public Post? Post { get; set; }
        }

public class Post {
            public int Id { get; set; }
            public string Content { get; set; }
            public int BlogId { get; set; }
            public Blog? Blog { get; set; }
            public List<Comment> Comments { get; set; } = own new(); // Wait, the prompt says "initialized to new()"
            // Actually, the prompt says "Collections List<Post> and List<Comment> must be initialized to new() in the entity class."
        }

public class Blog {
            public int Id { get; set; }
            public string Title { get; set; }
            public List<Post> Posts { get; set; } = new();
        }

public class BlogDbContext : DbContext {
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Post> Posts { get; set; }
            public DbSet<Comment> Comments { get; set; }

            public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

            protected override void OnModelCreating(ModelsBuilder modelBuilder) {
                // Configuration logic
            }
        }

public class BlogRepository {
            private readonly BlogDbContext _context;
            public BlogRepository(BlogDbContext context) { _context = context; }
            public async Task<Blog?> GetWithPostsAndCommentsAsync(int blogId) {
                // ...
            }
        }
s
public async Task<Blog?> GetWithPostsAndsCommentsAsync(int blogId) {
            return await _context.Blogs
                .Where(b => b.Id == blogId)
                .Include(b => b.Posts)
                .ThenInclude(p => p.Comments)
                .FirstOrDefaultAsync();
        }