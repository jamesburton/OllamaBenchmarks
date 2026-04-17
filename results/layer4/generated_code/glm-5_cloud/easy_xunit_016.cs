public interface IEventPublisher
{
    void Publish(string eventName);
}

public class OrderService
{
    private readonly IEventPublisher _eventPublisher;

    public OrderService(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public void PlaceOrder(string orderId)
    {
        _eventPublisher.Publish($"OrderPlaced:{orderId}");
    }
}