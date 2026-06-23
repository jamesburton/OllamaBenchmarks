namespace Contracts;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? AcceptedAt { get; set; }

    public void SetCompletedWhenFinalized()
    {
        this.SetProperty("Completed", "true");
    }
}

public event Event<OrderSubmitted> OrderSubmitted;
public event Event<OrderAccepted> OrderAccepted;
public event Event<OrderCompleted> OrderCompleted;

public static void SetCorrelationId(string correlationId, Guid orderId)
{
    var binding = new EventActivityBinder();
    binding.Event(() => { OrderSubmitted?.Invoke(orderId); });
    binding.Event(() => { OrderAccepted?.Invoke(orderId); });
    binding.Event(() => { OrderCompleted?.Invoke(orderId); });

    this.CorrelateById(m => m.OrderId, correlationId);
}

public static void SetCorrelationId(string correlationId, Guid orderId)
{
        var binding = new EventActivityBinder();
        binding.Event(() => { OrderSubmitted?.Invoke(orderId); });
        binding.Event(() => { OrderAccepted?.Invoke(orderId); });

        this.CorrelateById(m => m.OrderId, correlationId);
    }
}