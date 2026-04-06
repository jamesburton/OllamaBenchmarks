using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlogManagement
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
        public List<Comment> Comments { get; set; } = new();
    }

    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Post> Posts { get; set; } = new();
    }

    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasMany(b => b.Posts)
                    .WithOne(p => p.Blog)
                    .HasForeignKey(p => p.BlogId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasMany(p => p.Comments)
                    .WithOne(c => c.Post)
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

    public class BlogRepository
    {
        private readonly BlogDbContext _context;

        public BlogRepository(BlogDbContext context)
        {
            _context = context;
        }

        public async Task<Blog?> GetWithPostsAndCommentsAsync(int blogId)
        {
            return await _context.Blogs
                .Include(b => b.Posts)
                .ThenInclude(p => p.Comments)
                .FirstOrDefaultAsync(b => b.Id == blogId);
        }
    }
}