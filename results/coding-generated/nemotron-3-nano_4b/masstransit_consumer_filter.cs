using MassTransit;  
global using Contracts;  

namespace Contracts  
{  
    public record ProcessPayment(Guid OrderId, decimal Amount, string Currency);  
    public record PaymentProcessed(Guid OrderId, DateTime ProcessedAt);  

    public class ProcessPaymentConsumer : IConsumer<ProcessPayment>  
    {  
        public async Task Consume(ConsumeContext<ProcessPayment> context)  
        {  
            if (context.Message.Amount <= 0)  
                return;  

            var msg = new PaymentProcessed(context.Message.OrderId, DateTime.UtcNow);  
            await context.Publish(msg);  
        }  
    }  

    public class ProcessPaymentConsumerDefinition : ConsumerDefinition<ProcessPaymentConsumer>  
    {  
        public ProcessPaymentConsumer()  
        {  
            ConcurrentMessageLimit = 2;  
        }  

        protected override void ConfigureConsumer(  
            IReceiveEndpointConfigurator ep,  
            IConsumerConfigurator<ProcessPaymentConsumer> consumer,  
            IRegistrationContext ctx)  
        {  
            ep.UseMessageRetry(r => r.Interval(2, 500));  
        }  
    }  
}