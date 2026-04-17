using System;
using System.Collections.Generic;
using System.Linq;

public record CartItem(string ProductId, string Name, decimal Price, int Quantity);

public class ShoppingCart
{
    private readonly List<CartItem> _items = new();

    public List<CartItem> Items => _items;

    public void AddItem(CartItem item)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == item.ProductId);

        if (existingItem != null)
        {
            _items.Remove(existingItem);
            _items.Add(existingItem with { Quantity = existingItem.Quantity + item.Quantity });
        }
        else
        {
            _items.Add(item);
        }
    }

    public void RemoveItem(string productId)
    {
        _items.RemoveAll(i => i.ProductId == productId);
    }

    public decimal Total => _items.Sum(i => i.Price * i.Quantity);

    public int ItemCount => _items.Sum(i => i.Quantity);
}