using System;
using System.Collections.Generic;
using System.Text;

public interface IOrderRepository { }

public class OrderRepository : IOrderRepository
{
}

public interface IOrderService { }

public class OrderService : IOrderService
{
}

public interface IEmailNotifier { }

public class EmailNotifier : IEmailNotifier
{
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
        services.Configure<OrderSettings>(config.GetSection("Orders"));
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IEmailNotifier, EmailNotifier>();
        return services;
    }
}