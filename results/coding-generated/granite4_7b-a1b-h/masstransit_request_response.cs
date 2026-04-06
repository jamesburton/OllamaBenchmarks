using MassTransit;

namespace Contracts
{
    public record GetProduct(Guid ProductId);
    public record ProductResult(Guid ProductId, string Name, decimal Price);
    public record ProductNotFound();

    public class GetProductConsumer : IConsumer<GetProduct>
    {
        private static readonly Guid KnownId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public async Task Consume(ConsumeContext<GetProduct> context)
        {
            var productId = context.Message.ProductId;
            if (productId == KnownId)
            {
                await context.RespondAsync(new ProductResult(productId, "Test Widget", 19.99m));
            }
            else
            {
                await context.RespondAsync(new ProductNotFound());
            }
        }
    }
}