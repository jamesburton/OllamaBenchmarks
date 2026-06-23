using MassTransit;

namespace Contracts;
{
    public record SubmitOrder(Guid OrderId, string CustomerName, decimal Amount);

    public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);
}