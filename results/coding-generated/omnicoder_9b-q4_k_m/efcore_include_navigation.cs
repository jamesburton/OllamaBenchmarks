public class Comment
            {
                public int Id { get; set; }
                public string Text { get; set; }
                public int PostId { get; set; }
                public Post? Post { get; set; }
            }

public class Post
            {
                public int Id { get; set; }
                public string Content { get; set; }
                public int BlogId { get; set; }
                public Blog? Blog { get; set; }
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
                public DbSet<Blog> Blogs { get; set; }
                public DbSet<Post> Posts { get; set; }
                public DbSet<Comment> Comments { get; set; }

                public Blog(BlogDbContext) : base(options options ( options) { }
                public override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    // Configure relationships
                    modelBuilder.Entity<Blog>
                        .HasMany(b => b.Posts)
                        .WithMany(p => p.Blog) // Wait, EF Core convention usually handles this if properties exist.
                        // But explicit configuration is safer given the prompt asks for "Blog has many Posts (with foreign key BlogId)"
                        // Actually, standard EF Core with navigation properties usually infers relationships.
                        // However, the prompt says "OnModelCreating override that configures: Blog has many Posts (with foreign key BlogId), Post has many Comments (with foreign key PostId)".
                        // I should use modelBuilder.Entity to configure these.
                }
            }

modelBuilder.Entity<Post>()
                .HasOne(p => p.Blog)
                .WithMany(b => b.Posts)
                .HasForeignKey(p => p.BlogId);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId);