public class OrderSettings { public string WarehouseCode { get; set; } public int MaxRetries { get; set; } }

public interface IOrderRepository { }
public interface IOrderService { }
public interface IEmailNotifier { }

public class OrderRepository : IOrderRepository { public OrderRepository() {} }

public class OrderService : IOrderService { public OrderService() {} }

public class EmailNotifier : IEmailNotifier { public EmailNotifier() {} }

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