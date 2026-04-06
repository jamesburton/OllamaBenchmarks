using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    protected override void OnModelCreating(ModelBuilder