using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

interface IOrderRepository { }

class OrderRepository : IOrderRepository
{
    public OrderRepository()
    {
    }

    public OrderSettings Settings { get; }
    public OrderRepository(OrderSettings settings) => Settings = settings;
}

interface IOrderService { }

class OrderService : IOrderService
{
    public OrderService() => Settings = OrderSettings.Default;
    public OrderSettings Settings { get; }
    public static OrderSettings Default => new OrderSettings();
}

interface IEmailNotifier { }

class EmailNotifier : IEmailNotifier
{
    public EmailNotifier() => Settings = OrderSettings.Default;
    public OrderSettings Settings { get; }
    public static OrderSettings Default => new OrderSettings();
}

class OrderSettings
{
    public string WarehouseCode { get; set; } = "";
    public int MaxRetries { get; set; } = 3;
}

public static class ServiceCollectionExtensions
{
    public static void AddOrderServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IEmailNotifier, EmailNotifier>();
        services.Configure<OrderSettings>(config.GetSection("Orders"));
    }
}