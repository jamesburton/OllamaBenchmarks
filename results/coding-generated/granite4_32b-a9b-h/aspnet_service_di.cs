using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// 1. IOrderRepository interface
public interface IOrderRepository { }

// 2. OrderRepository class implementing IOrderRepository
public class OrderRepository : IOrderRepository
{
    public OrderRepository() { }
}

// 3. IOrderService interface
public interface IOrderService { }

// 4. OrderService class implementing IOrderService
public class OrderService : IOrderService
{
    public OrderService() { }
}

// 5. IEmailNotifier interface
public interface IEmailNotifier { }

// 6. EmailNotifier class implementing IEmailNotifier
public class EmailNotifier : IEmailNotifier
{
    public EmailNotifier() { }
}

// 7. OrderSettings class
public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

// ServiceCollectionExtensions static class with extension method
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Register IOrderRepository -> OrderRepository as scoped
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Register IOrderService -> OrderService as scoped
        services.AddScoped<IOrderService, OrderService>();

        // Register IEmailNotifier -> EmailNotifier as transient
        services.AddTransient<IEmailNotifier, EmailNotifier>();

        // Bind OrderSettings from config section "Orders" using the options pattern
        services.Configure<OrderSettings>(config.GetSection("Orders"));

        return services;
    }
}