using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Comment {
    public int Id { get; set; }
    public string Text { get; set; }
    public int PostId { get; set; } // Nullable to avoid EF Core warnings: Blog? Blog
    public Post? Post { get; set; } = new(); // Initialized with new()
}

public class Post {
    public int Id { get; set; }
    public string Content { get; set; }
    public int BlogId { get; set; } // Nullable to avoid EF Core warnings: Blog? Blog
    public Blog? Blog { get; set; } = new(); // Initialized with new()
    public List<Comment>? Comments { get; set; } = new(); // Initialized with new() and nullable
}

public class Blog {
    public int Id { get; set; }
    public string Title { get; set; }
    public List<Post> Posts { get; set; } = new(); // Initialized with new()
}

public partial class BlogDbContext : DbContext {
    public DbSet<Blog> Blogs { get; private set; }
    public DbSet<Post> Posts { get; private set; }
    public DbSet<Comment> Comments { get; private set; }

    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Blog>()
            .HasMany(b => b.Posts)
            .WithOne(p => p.Blog)
            .HasForeignKey("BlogId");

        modelBuilder.Entity<Post>()
            .HasMany(c => c.Comments)
            .WithOne(co => co.Comment)
            .HasForeignKey("PostId");
    }
}

public class BlogRepository {
    private readonly BlogDbContext _context;

    public BlogRepository(BlogDbContext context) {
        _context = context;
    }

    public async Task<Blog?> GetWithPostsAndCommentsAsync(int blogId) {
        return await _context.Blogs
            .Include(b => b.Posts)
                .ThenInclude(p => p.Comments)
            .FirstOrDefaultAsync(b => b.Id == blogId);
    }
}