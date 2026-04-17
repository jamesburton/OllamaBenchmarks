using System;
using System.Collections.Generic;
using System.Linq;

public record MenuItem(int Id, string Name, decimal Price, string Category);

public class Menu
{
    private readonly List<MenuItem> _items = new();

    public void AddItem(MenuItem item)
    {
        _items.Add(item);
    }

    public void RemoveItem(int id)
    {
        _items.RemoveAll(i => i.Id == id);
    }

    public List<MenuItem> GetByCategory(string category)
    {
        return _items.Where(i => i.Category == category).ToList();
    }

    public decimal AveragePrice => _items.Count == 0 ? 0 : _items.Average(i => i.Price);
}