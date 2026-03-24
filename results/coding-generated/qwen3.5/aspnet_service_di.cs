using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MyApp;

public interface IOrderRepository { }

public class OrderRepository : IOrderRepository { }

public interface IOrderService { }

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailNotifier _notifier;

    public OrderService(IOrderRepository repository, IEmailNotifier notifier)
    {
        _repository = repository;
        _notifier = notifier;
    }
}

public interface IEmailNotifier { }

public class EmailNotifier : IEmailNotifier
{
    private readonly IOptions<OrderSettings> _settings;

    public EmailNotifier(IOptions<OrderSettings> settings)
    {
        _settings = settings;
    }
}

public class OrderSettings
{
    public string WarehouseCode { get; set; } = string.Empty;
    public int MaxRetries { get; set; }
}

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