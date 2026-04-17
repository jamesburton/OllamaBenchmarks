using System;
using System.Collections.Generic;
using System.Linq;

public record StockItem(string Sku, int Quantity, int ReorderPoint);

public class WarehouseService
{
    private readonly Dictionary<string, StockItem> _inventory = new();

    public void AddItem(StockItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (_inventory.ContainsKey(item.Sku))
        {
            throw new InvalidOperationException($"Item with SKU '{item.Sku}' already exists.");
        }

        _inventory[item.Sku] = item;
    }

    public void Receive(string sku, int quantity)
    {
        if (string.IsNullOrEmpty(sku))
        {
            throw new ArgumentException("SKU cannot be null or empty.", nameof(sku));
        }

        if (!_inventory.TryGetValue(sku, out var item))
        {
            throw new KeyNotFoundException($"Item with SKU '{sku}' not found.");
        }

        _inventory[sku] = item with { Quantity = item.Quantity + quantity };
    }

    public bool Dispatch(string sku, int quantity)
    {
        if (string.IsNullOrEmpty(sku))
        {
            throw new ArgumentException("SKU cannot be null or empty.", nameof(sku));
        }

        if (!_inventory.TryGetValue(sku, out var item))
        {
            return false;
        }

        if (item.Quantity < quantity)
        {
            return false;
        }

        _inventory[sku] = item with { Quantity = item.Quantity - quantity };
        return true;
    }

    public List<StockItem> GetLowStock()
    {
        return _inventory.Values
                         .Where(item => item.Quantity <= item.ReorderPoint)
                         .ToList();
    }
}