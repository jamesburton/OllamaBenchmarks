global using Contracts;

using MassTransit;
using System;

namespace Contracts;

public record GetProduct(Guid ProductId);

public record ProductResult(Guid ProductId, string Name, decimal Price);

public record ProductNotFound();

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