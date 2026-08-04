using MassTransit;
using Contracts;

namespace MassTransit
{
    public class SubmitOrder : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderSubmitted(msg.Guid, DateTime.UtcNow));
        }
    }

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

    public class SubmitOrderConsumer : IConsumer<OrderConsumer>
    {
        public async Task Consume(ConsumeContext<OrderConsumer> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderProcessed(msg.OrderId));
        }
    }

    public class OrderConsumerDefinition : ConsumerDefinition<OrderConsumer>
    {
        protected override void ConfigureConsumer(
            IConsumerConfigurator<OrderConsumer> consumer,
            IRegistrationContext ctx)
        {
            consumer.ConsumeContext.PublishInMemory();
        }
    }

    public class OrderConsumer : IConsumer<OrderPlaced>
    {
        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            var msg = context.Message;
            await context.Publish(new OrderPlaced(msg.Guid, DateTime.UtcNow));
        }
    }

    public class OrderPlaced : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
    }

    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Processing { get; private set; }
        public Event<OrderPlaced> OrderPlaced { get; private set; }

        public OrderStateMachine()
        {
            Initially(When(OrderPlaced).TransitionTo(Processing)));
        }

        public void During(Processing, When(OrderShipped).Finalize())
        {
            OrderPlaced.Publish();
        }
    }

    public class SubmitOrderTestHarness : ITestHarness
    {
        public SubmitOrderTestHarness(IClient<ITestHarness> client)
        {
            client.AddConsumer<OrderConsumer>();
            client.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
        }

        public void ConsumeOrder()
        {
            var harness = provider.GetRequiredService<ITestHarness>();
            await harness.Start();
            await harness.Bus.Publish(new OrderPlaced(Guid.NewGuid(), "John Doe", 100.0m));
            Assert.True(await harness.Consumed.Any<OrderPlaced>());
        }
    }
}