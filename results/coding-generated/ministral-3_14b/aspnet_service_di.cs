using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public interface IOrderRepository { }

public class OrderRepository : IOrderRepository
{
    // Implementation would go here
}

public interface IOrderService { }

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEmailNotifier _emailNotifier;

    public OrderService(IOrderRepository orderRepository, IEmailNotifier emailNotifier)
    {
        _orderRepository = orderRepository;
        _emailNotifier = emailNotifier;
    }
}

public interface IEmailNotifier { }

public class EmailNotifier : IEmailNotifier
{
    // Implementation would go here
}

public class OrderSettings
{
    public string WarehouseCode { get; set; } = string.Empty;
    public int MaxRetries { get; set; }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<OrderSettings>(config.GetSection("Orders"));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IEmailNotifier, EmailNotifier>();

        return services;
    }
}