public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEmailSender _emailSender;

    public OrderService(IOrderRepository orderRepository, IEmailSender emailSender)
    {
        _orderRepository = orderRepository;
        _emailSender = emailSender;
    }

    public async Task<CreateOrderRequest> ValidateAndCreate(CreateOrderRequest request)
    {
        if (!request.IsValid())
            throw new ArgumentException("Invalid request", nameof(request));

        var order = await _orderRepository.CreateAsync(request);
        await _emailSender.SendEmailAsync(
            "New Order",
            $"A new order has been received for {order.Name} at ${order.Price}. Please review the details.",
            "noreply@example.com");
        return order;
    }
}