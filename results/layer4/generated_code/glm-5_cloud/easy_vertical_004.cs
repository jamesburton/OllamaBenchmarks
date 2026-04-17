using System;
using System.Collections.Generic;
using System.Linq;

public record Category(int Id, string Name);

public record Article(int Id, string Title, int CategoryId);

public class ArticleCatalog
{
    private readonly List<Category> _categories = new();
    private readonly List<Article> _articles = new();

    public void AddCategory(Category category)
    {
        if (category == null)
        {
            throw new ArgumentNullException(nameof(category));
        }
        _categories.Add(category);
    }

    public void AddArticle(Article article)
    {
        if (article == null)
        {
            throw new ArgumentNullException(nameof(article));
        }
        _articles.Add(article);
    }

    public List<Article> GetByCategory(int categoryId)
    {
        return _articles.Where(a => a.CategoryId == categoryId).ToList();
    }

    public int ArticleCount => _articles.Count;
}