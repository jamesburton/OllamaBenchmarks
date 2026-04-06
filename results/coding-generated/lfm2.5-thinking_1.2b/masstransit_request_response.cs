global using Contracts;
using MassTransit;
using System;

namespace Contracts {
    public class GetProductRequest { /* ... */ }
    public class ProductResult { /* ... */ }
    public class ProductNotFound { /* ... */ }
    public class GetProductConsumer : IConsumer<GetProduct> {
        private static readonly Guid KnownId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public async Task Consume(ConsumeContext<GetProductRequest> context) {
            var msg = context.Message;
            if (msg.ProductId == KnownId) {
                return new ProductResult(msg.ProductId, "Test Widget", 19.99m);
            } else {
                return new ProductNotFound();
            }
        }
    }
}