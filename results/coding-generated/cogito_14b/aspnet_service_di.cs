using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

public interface IOrderRepository { }

public class OrderRepository : IOrderRepository { }

public interface IOrderService { }

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IEmailNotifier _emailNotifier;

    public OrderService(IOrderRepository repository, IEmailNotifier emailNotifier)
    {
        _repository = repository;
        _emailNotifier = emailNotifier;
    }
}

public interface IEmailNotifier { }

public class EmailNotifier : IEmailNotifier
{
    public void Notify(string message) { }
}

public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderServices(
        this IServiceCollection services, 
        IConfiguration config)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IEmailNotifier, EmailNotifier>();
        services.Configure<OrderSettings>(config.GetSection("Orders"));
        return services;
    }
}