using MassTransit;

namespace Contracts;
{
            public static Guid KnownId => Guid.Parse("11111111-1111-1111-1111-111111111111");

            public record GetProduct(string ProductId);
            public record ProductResult(Guid ProductId, string Name, decimal Price);

            public static void RegisterGetProductConsumer() => 
                services.AddMassTransit(x =>
                {
                    x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
                    x.AddConsumer<GetProductConsumer>();
                });

            public class GetProductConsumer : IConsumer<GetProduct>
            {
                protected override async Task Consume(ConsumeContext<GetProduct> context)
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

            public record ProductNotFound();
        }