using MassTransit;

// Global using directive to make types in the Contracts namespace accessible
global using Contracts;

namespace Contracts;

// 1. GetProduct request record
public record GetProduct(Guid ProductId);

// 2. ProductResult response record
public record ProductResult(Guid ProductId, string Name, decimal Price);

// 3. ProductNotFound response record
public record ProductNotFound();

// 4. GetProductConsumer class
public class GetProductConsumer : IConsumer<GetProduct>
{
    // Hardcoded known product ID
    public static readonly Guid KnownId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public async Task Consume(ConsumeContext<GetProduct> context)
    {
        var message = context.Message;

        if (message.ProductId == KnownId)
        {
            // Respond with ProductResult
            await context.RespondAsync(new ProductResult(KnownId, "Test Widget", 19.99m));
        }
        else
        {
            // Respond with ProductNotFound
            await context.RespondAsync(new ProductNotFound());
        }
    }
}