model.Entity<Blog>()
                    .HasMany(b => b.Posts)
                    .WithOne(p => p.Blog)
                    .HasForeignKey(p => p.BlogId)
                    .OnDelete(DeleteBehavior.Cascade); // Optional but good

model.Entity<Post>()
                    .HasMany(p => p.Comments)
                    .WithOne(c => c.Post)
                    .HasForeignKey(c => c.PostId)
                    .IsRequired(); // PostId is int, usually required, but Post is nullable on Comment.