global using Contracts;

using System;
using MassTransit;

namespace MyApplication // Root namespace to satisfy the requirement of having a namespace containing the types, 
                       // while keeping 'Contracts' as the specific namespace for message types if desired by convention.
// However, strictly following "Place all types inside `namespace Contracts;`" implies this should be the only or primary one.
// To ensure valid C# and strict adherence:

namespace Contracts;

public record GetProduct(Guid ProductId);

public record ProductResult(
    Guid ProductId, 
    string Name, 
    decimal Price
);

public record ProductNotFound();

public class GetProductConsumer : IConsumer<GetProduct>
{
    public static readonly Guid KnownId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public async Task Consume(ConsumeContext<GetProduct> context)
    {
        if (context.Message.ProductId == KnownId)
        {
            await context.RespondAsync<ProductResult>(new ProductResult(KnownId, "Test Widget", 19.99m));
        }
        else
        {
            await context.RespondAsync<ProductNotFound>();
        }
    }
}