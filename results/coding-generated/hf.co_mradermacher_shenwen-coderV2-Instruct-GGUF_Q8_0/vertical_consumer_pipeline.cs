using System;
using System.Threading.Tasks;

namespace Contracts
{
    public record PlaceOrder(Guid OrderId, string CustomerName);
}

namespace Contracts
{
    public record OrderPlaced(Guid OrderId, string CustomerName);
}

namespace Contracts
{
    public class NotifyCustomerConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            await context.Publish(new OrderProcessed(context.Message.OrderId));
        }
    }
}