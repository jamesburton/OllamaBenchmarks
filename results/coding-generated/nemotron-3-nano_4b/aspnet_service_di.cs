using System;

// Interface
interface IOrderRepository { }

// Interface
interface IOrderService { }

// Interface
interface IEmailNotifier { }

// Class
class OrderRepository : IOrderRepository { }

// Class
class OrderService : IOrderService { }

// Class
class EmailNotifier : IEmailNotifier { }

// Class
class OrderSettings {
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

// Static extensions
static class ServiceCollectionExtensions {
    public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration config) {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IEmailNotifier, EmailNotifier>();
        services.Configure<OrderSettings>();
        return services;
    }
}