using MassTransit;

namespace Contracts;

global using Contracts;

public record GetProduct(Guid ProductId);
public record ProductResult(Guid ProductId, string Name, decimal Price);
public record ProductNotFound();

public static class ProductMessages
{
    public static Guid KnownId = Guid.Parse("11111111-1111-1111-1111-111111111111");
}

public class GetProductConsumer : IConsumer<GetProduct>
{
    public static Guid KnownId { get; } = ProductMessages.KnownId;

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