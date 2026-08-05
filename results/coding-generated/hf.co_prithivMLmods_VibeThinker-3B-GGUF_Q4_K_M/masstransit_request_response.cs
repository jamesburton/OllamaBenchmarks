using MassTransit;
global using Contracts;

namespace Contracts {
    // Request message record
    public record GetProduct(Guid ProductId);

    // Response message records
    public record ProductResult(Guid ProductId, string Name, decimal Price);
    public record ProductNotFound();

    // Consumer implementation for GetProduct
    public class GetProductConsumer : IConsumer<GetProduct> {
        private static readonly Guid KnownId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public