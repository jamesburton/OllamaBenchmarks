using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// Marker interfaces
public interface IOrderRepository { }

public interface IOrderService { }

public interface IEmailNotifier { }

// Implementations with parameterless constructors
public class OrderRepository : IOrderRepository
{
    public OrderRepository() { }
}

public class OrderService : IOrderService
{
    public OrderService() { }
}

public class EmailNotifier : IEmailNotifier
{
    public EmailNotifier() { }
}

// Options class
public class OrderSettings
{
    public string WarehouseCode { get; set; } = string.Empty;
    public int MaxRetries { get; set; }
}

// Extension method for service registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration config)
    {
        // Register services with appropriate lifetimes
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IEmailNotifier, EmailNotifier>();

        // Bind OrderSettings from the "Orders" configuration section
        services.Configure<OrderSettings>(config.GetSection("Orders"));

        return services;
    }
}