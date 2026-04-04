public interface IOrderRepository { }
public class OrderRepository : IOrderRepository { }

public interface IOrderService { }
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository) => _repository = repository;

    public void ProcessOrder(string name, int qty)
    {
        // Implementation not required
    }
}

public interface IEmailNotifier { }
public class EmailNotifier : IEmailNotifier
{
    public void SendEmail(string message) => Console.WriteLine(message);
}

public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IEmailNotifier, EmailNotifier>();
        services.Configure<OrderSettings>(config.GetSection("Orders"));

        return services;
    }
}