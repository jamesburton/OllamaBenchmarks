using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int ViewCount { get; set; }
    public DateTime PublishedAt { get; set; }
}

public class ArticleContext : DbContext
{
    public DbSet<Article> Articles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("ArticleDatabase");
    }
}

public static class ArticleRepository
{
    public static List<Article> GetTopByViews(ArticleContext context, int count)
    {
        return context.Articles
            .OrderByDescending(a => a.ViewCount)
            .Take(count)
            .ToList();
    }
}