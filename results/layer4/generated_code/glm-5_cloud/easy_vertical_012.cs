public record Inventory(string Sku, string Name, int Quantity);

public class InventoryManager
{
    private readonly Dictionary<string, Inventory> _inventory = new();

    public void AddStock(Inventory item)
    {
        if (_inventory.ContainsKey(item.Sku))
        {
            var existing = _inventory[item.Sku];
            _inventory[item.Sku] = existing with { Quantity = existing.Quantity + item.Quantity };
        }
        else
        {
            _inventory[item.Sku] = item;
        }
    }

    public bool Deduct(string sku, int quantity)
    {
        if (!_inventory.TryGetValue(sku, out var item) || item.Quantity < quantity)
        {
            return false;
        }

        _inventory[sku] = item with { Quantity = item.Quantity - quantity };
        return true;
    }

    public int GetQuantity(string sku)
    {
        return _inventory.TryGetValue(sku, out var item) ? item.Quantity : 0;
    }
}