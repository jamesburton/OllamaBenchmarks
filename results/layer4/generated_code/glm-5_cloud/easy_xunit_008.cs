using System;
using System.Collections.Generic;

public interface IRepository<T>
{
    void Add(T item);
    T? GetById(int id);
    IEnumerable<T> GetAll();
}

public record Product(int Id, string Name);

public class ProductRepository : IRepository<Product>
{
    private readonly List<Product> _items = new();

    public void Add(Product item)
    {
        _items.Add(item);
    }

    public Product? GetById(int id)
    {
        return _items.Find(p => p.Id == id);
    }

    public IEnumerable<Product> GetAll()
    {
        return _items;
    }
}