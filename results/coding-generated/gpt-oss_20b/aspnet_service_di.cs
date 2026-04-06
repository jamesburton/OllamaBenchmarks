using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// 1. Marker interface for order repository
public interface IOrderRepository { }

// 2. Concrete implementation with a parameterless constructor
public class OrderRepository : IOrderRepository
{
    public OrderRepository() { }
}

// 3. Marker interface for order service
public interface IOrderService { }

// 4. Concrete implementation with a parameterless constructor
public class OrderService : IOrderService
{
    public OrderService() { }
}

// 5. Marker interface for email notifier
public interface IEmailNotifier { }

// 6. Concrete implementation with a parameterless constructor
public class EmailNotifier : IEmailNotifier
{
    public EmailNotifier() { }
}

// 7. Settings class for orders
public class OrderSettings
{
    public string WarehouseCode { get; set; } = string.Empty;
    public int MaxRetries { get; set; }
}

// 8. Extension method to register services and bind options
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