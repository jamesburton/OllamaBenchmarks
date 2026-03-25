using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrderDemo;

#region Interfaces (marker)

public interface IOrderRepository { }
public interface IOrderService { }
public interface IEmailNotifier { }

#endregion

#region Implementations

public class OrderRepository : IOrderRepository { }

public class OrderService : IOrderService
{
    public OrderService() { }
}

public class EmailNotifier : IEmailNotifier
{
    public EmailNotifier() { }
}

#endregion

#region Settings

public class OrderSettings
{
    public string WarehouseCode { get; set; } = string.Empty;
    public int MaxRetries { get; set; }
}

#endregion

#region ServiceCollection Extension

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration config)
    {
        // Bind OrderSettings from the "Orders" configuration section
        services.Configure<OrderSettings>(config.GetSection("Orders"));

        // Register the repository, service, and notifier
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IEmailNotifier, EmailNotifier>();

        return services;
    }
}

#endregion