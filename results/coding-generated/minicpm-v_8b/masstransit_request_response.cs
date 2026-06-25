using System;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts
{
    public class GetProductRequest : IRequest<ProductResult>
    {
        public Guid ProductId { get; set; }
    }

    public record ProductResult(Guid ProductId, string Name, decimal Price);

    public record ProductNotFound();

    public class GetProductConsumer : IConsumer<GetProduct>
    {
        private readonly Guid _knownProductId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public async Task Consume(ConsumeContext<GetProduct> context)
        {
            if (context.Message.ProductId == _knownProductId)
            {
                await context.RespondAsync(new ProductResult(_knownProductId, "Test Widget", 19.99m));
            }
            else
            {
                await context.RespondAsync(new ProductNotFound());
            }
        }
    }
}