using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public interface IOrderRepository { }

public class OrderRepository : IOrderRepository
{
    public OrderRepository() { } // Parameterless constructor
}

public interface IOrderService { }

public class OrderService : IOrderService
{
    public OrderService() { } // Parameterless constructor
}

public interface IEmailNotifier { }

public class EmailNotifier : IEmailNotifier
{
    public EmailNotifier() { } // Parameterless constructor
}

public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
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