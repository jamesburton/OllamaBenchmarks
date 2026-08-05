public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

// IOrderRepository.cs
public interface IOrderRepository
{
    Task<IOrder> GetAllOrdersAsync();
}

public class OrderRepository : IOrderRepository
{
    private readonly IOrderRepository _orderRepository;

    public OrderRepository(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IOrder> GetAllOrdersAsync()
    {
        return _orderRepository.GetAllOrdersAsync();
    }
}

// IOrderService.cs
public interface IOrderService
{
    Task<IOrder> PlaceOrderAsync(string name, int qty);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IOrder> PlaceOrderAsync(string name, int qty)
    {
        return _orderRepository.PlaceOrderAsync(name, qty);
    }
}

// IEmailNotifier.cs
public interface IEmailNotifier
{
    void SendEmailAsync(string to, string subject, string body);
}

public class EmailNotifier : IEmailNotifier
{
    private readonly IEmailNotifier _emailNotifier;

    public EmailNotifier(IEmailNotifier emailNotifier)
    {
        _emailNotifier = emailNotifier;
    }

    public async void SendEmailAsync(string to, string subject, string body)
    {
        await _emailNotifier.SendEmailAsync(to, subject, body);
    }
}

// ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static void AddOrderServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddRepository<OrderRepository>();
        services.AddService<OrderService>();
        services.AddNotifier<IEmailNotifier>();
        services.AddSettings<OrderSettings>();
    }
}