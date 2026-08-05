using System;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts
{
    public class ProcessPayment : IMassTransitRequest { }
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public Task Send() => default;
    }

    public event EventHandler<ProcessPayment> Received;

    [Serializable]
    public class PaymentProcessed : INotification
    {
        public Guid OrderId { get; set; }
        public DateTime ProcessedAt { get; set; }

        public void Handle(ProcessPayment message)
        {
            if (message.Amount <= 0) return;
            Received.Invoke(message);
            await context.Publish(new PaymentProcessed(message.OrderId, DateTime.UtcNow));
        }
    }
}

public class OrderConsumer : IConsumer<ProcessPayment>
{
    private readonly IMassTransitContext _context;

    public OrderConsumer(IMassTransitContext context)
    {
        _context = context;
    }

    protected async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var msg = context.Message;
        if (msg.Amount <= 0) return;
        await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow));
    }
}

public class OrderConsumerDefinition : ConsumerDefinition<OrderConsumer>
{
    public OrderConsumerDefinition()
    {
        ConfigureConsumer(_context);
    }

    protected void ConfigureConsumer(IMassTransitContext context)
    {
        var ep = context.GetEndpoint<ProcessPayment>();
        ep.UseMessageRetry(r => r.Interval(2, 500));
        ep.UseInMemoryOutbox();
        ep.SetConcurrentMessageLimit(2);
    }
}

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    protected override async Task OnStateEnteredAsync(string state)
    {
        switch (state)
        {
            case "Processing":
                await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow));
                break;
        }
    }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Processing { get; private set; }

    protected override void ConfigureSaga(OrderState stateMachineState)
    {
        base.ConfigureSaga(stateMachineState);
        Processing = new State("Processing", async () => await context.Publish(new PaymentProcessed(msg.OrderId, DateTime.UtcNow)));
    }
}