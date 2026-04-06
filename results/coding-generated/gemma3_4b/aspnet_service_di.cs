public interface IOrderRepository {}

public class OrderRepository
{
    // No constructor dependencies
}

public interface IOrderService {}

public class OrderService
{
    // No constructor dependencies
}

public interface IEmailNotifier {}

public class EmailNotifier
{
    // No constructor dependencies
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
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddTransient<IEmailNotifier, EmailNotifier>();
        services.Configure<OrderSettings>(config.GetSection("Orders"));
        return services;
    }
}