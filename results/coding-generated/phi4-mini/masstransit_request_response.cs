using System;
using MassTransit;

global using Contracts;

// Define messages with positional records and namespaces

public record GetProduct(Guid ProductId);

public record ProductResult(Guid ProductId, string Name, decimal Price);

// Response when product is not found
public record ProductNotFound();

// Consumer implementation for handling the GetProduct message
public class GetProductConsumer : IConsumer<GetProduct>
{
    public static readonly Guid KnownId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public async Task Consume(ConsumeContext<GetProduct> context)
    {
        if (context.Message.ProductId == KnownId)
        {
            await context.RespondAsync(new ProductResult(KnownId, "Test Widget", 19.99m));
        }
        else
        {
            await context.RespondAsync(new ProductNotFound());
        }
    }
}

// Define the namespace for contracts to contain all message types and consumer implementations.
namespace Contracts
{
}