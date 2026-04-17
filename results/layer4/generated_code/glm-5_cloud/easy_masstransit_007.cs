using System;
using System.Collections.Generic;

namespace Contracts
{
    public record ProductPriceChanged(string ProductId, decimal OldPrice, decimal NewPrice);
}

public interface IConsumer<T>
{
    Task Consume(T message);
}

public class ProductPriceChangedConsumer : IConsumer<Contracts.ProductPriceChanged>
{
    public List<Contracts.ProductPriceChanged> Changes = new();

    public Task Consume(Contracts.ProductPriceChanged message)
    {
        if (message.NewPrice != message.OldPrice)
        {
            Changes.Add(message);
        }

        return Task.CompletedTask;
    }
}