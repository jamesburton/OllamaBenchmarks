using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Marker interface for OrderRepository
public interface IOrderRepository { }

// Implementation of IOrderRepository with parameterless constructor
public class OrderRepository : IOrderRepository
{
    public OrderRepository() { }
}

// Marker interface for OrderService
public interface IOrderService { }

// Implementation of IOrderService with parameterless constructor
public class OrderService : IOrderService
{
    public OrderService() { }
}

// Marker interface for EmailNotifier
public interface IEmailNotifier { }

// Implementation of IEmailNotifier with parameterless constructor
public class EmailNotifier : IEmailNotifier
{
    public EmailNotifier() { }
}

// Class to hold order settings
public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

// Static class for service collection extensions
public static class ServiceCollectionExtensions
{
    // Extension method to add order services and configure options
    public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration config)
    {
        // Register IOrderRepository with OrderRepository as scoped
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Register IOrderService with OrderService as scoped
        services.AddScoped<IOrderService, OrderService>();

        // Register IEmailNotifier with EmailNotifier as transient
        services.AddTransient<IEmailNotifier, EmailNotifier>();

        // Bind OrderSettings from configuration section "Orders"
        services.Configure<OrderSettings>(config.GetSection("Orders"));

        return services;
    }
}