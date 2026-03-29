using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

// 1. IOrderRepository interface
public interface IOrderRepository
{
}

// 2. OrderRepository class
public class OrderRepository : IOrderRepository
{
}

// 3. IOrderService interface
public interface IOrderService
{
}

// 4. OrderService class
public class OrderService : IOrderService
{
}

// 5. IEmailNotifier interface
public interface IEmailNotifier
{
}

// 6. EmailNotifier class
public class EmailNotifier : IEmailNotifier
{
}

// 7. OrderSettings class
public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

// 8. ServiceCollectionExtensions extension method
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