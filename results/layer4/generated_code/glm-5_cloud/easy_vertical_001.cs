using System;
using System.Collections.Generic;
using System.Linq;

public record Product(int Id, string Name, decimal Price);

public interface IProductRepository
{
    void Add(Product product);
    Product? GetById(int id);
    List<Product> GetAll();
}

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();

    public void Add(Product product)
    {
        _products.Add(product);
    }

    public Product? GetById(int id)
    {
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public List<Product> GetAll()
    {
        return new List<Product>(_products);
    }
}