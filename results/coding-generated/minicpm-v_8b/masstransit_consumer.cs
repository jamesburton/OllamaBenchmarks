using System;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts
{
    public class SubmitOrder : IEvent
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
    }

    public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt);

    [MessageContract(IsInitiator = true)]
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderSubmitted(msg.OrderId, DateTime.UtcNow));
        }
    }

    [ConsumerDefinition]
    public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
    {
        protected override void ConfigureConsumer(
            IReceiveEndpointConfigurator ep,
            IConsumerConfigurator<SubmitOrderConsumer> consumer,
            IRegistrationContext ctx)
        {
            ep.UseMessageRetry(r => r.Interval(3, 1000));
            ep.UseInMemoryOutbox(ctx);
        }
    }

    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }

        protected override void ConfigureSaga(SagaStateMachine<SubmitOrderConsumer> stateMachine)
        {
            stateMachine.When<SubmitOrder>().Then(state =>
                {
                    if (state.State == "Processing")
                        state.SetCurrentState("Submitted");
                });
        }
    }

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        protected override void ConfigureSaga(SagaStateMachine<SubmitOrderConsumer> stateMachine)
        {
            stateMachine.When<SubmitOrder>().Then(state =>
                {
                    if (state.State == "Processing")
                        state.SetCurrentState("Submitted");
                });
        }
    }

    public class OrderService : IHostedService
    {
        private readonly IBus _bus;

        public OrderService(IBus bus)
        {
            _bus = bus;
        }

        public async Task SubmitOrderAsync(SubmitOrder order)
        {
            await _bus.Publish(order);
        }
    }
}