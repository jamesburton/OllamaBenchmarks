using System.Collections.Generic;

namespace Contracts
{
    public record InventoryUpdated(string ProductId, int NewQuantity, string Warehouse);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class InventoryUpdatedConsumer : IConsumer<Contracts.InventoryUpdated>
{
    public Dictionary<string, int> Inventory = new();

    public void Consume(Contracts.InventoryUpdated message)
    {
        Inventory[message.ProductId] = message.NewQuantity;
    }
}