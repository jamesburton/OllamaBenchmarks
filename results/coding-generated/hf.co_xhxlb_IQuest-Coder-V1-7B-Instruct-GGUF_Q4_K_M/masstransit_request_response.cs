using System;
using System.Threading.Tasks;
using MassTransit;
using Contracts;

namespace Contracts
{
    public record GetProduct(Guid ProductId);
    public record ProductResult(Guid ProductId, string Name, decimal Price);
    public record ProductNotFound();
}

namespace Contracts
{
    global using MassTransit;

    namespace Contracts
    {
        public class ProductService
        {
            public static Guid KnownId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        }
    }

    public class GetProductConsumer : IConsumer<GetProduct>
    {
        public async Task Consume(ConsumeContext<GetProduct> context)
        {
            if (context.Message.ProductId == Contracts.ProductService.KnownId)
            {
                await context.RespondAsync(new ProductResult(
                    context.Message.ProductId, 
                    "Test Widget", 
                    19.99m));
            }
            else
            {
                await context.RespondAsync(new ProductNotFound());
            }
        }
    }
}