using MassTransit;
global using Contracts;

namespace Contracts;

// 1. GetProduct request record (positional)
public record GetProduct(Guid ProductId);

// 2. ProductResult response record (positional)
public record ProductResult(Guid ProductId, string Name, decimal Price);

// 3. ProductNotFound response record (no properties)
public record ProductNotFound();

// 4. GetProductConsumer implementing IConsumer<GetProduct>
public class GetProductConsumer : IConsumer<GetProduct>
{
    public static readonly Guid KnownId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public async Task Consume(ConsumeContext<GetProduct> context)
    {
        var msg = context.Message;
        if (msg.ProductId == KnownId)
        {
            await context.RespondAsync<ProductResult>(new ProductResult(msg.ProductId, "Test Widget", 19.99m));
        }
        else
        {
            await context.RespondAsync<ProductNotFound>();
        }
    }
}