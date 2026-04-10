using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

// 1. IOrderRepository interface
public interface IOrderRepository { }

// 2. OrderRepository class
public class OrderRepository : IOrderRepository
{
    // Parameterless constructor
}

// 3. IOrderService interface
public interface IOrderService { }

// 4. OrderService class
public class OrderService : IOrderService
{
    // Parameterless constructor
}

// 5. IEmailNotifier interface
public interface IEmailNotifier { }

// 6. EmailNotifier class
public class EmailNotifier : IEmailNotifier
{
    // Parameterless constructor
}

// 7. OrderSettings class
public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

// 8. ServiceCollectionExtensions static class
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration config)
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