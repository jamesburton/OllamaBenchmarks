using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// 1. IOrderRepository interface
public interface IOrderRepository
{
}

// 2. OrderRepository implementing IOrderRepository
public class OrderRepository : IOrderRepository
{
    public OrderRepository() { }
}

// 3. IOrderService interface
public interface IOrderService
{
}

// 4. OrderService implementing IOrderService
public class OrderService : IOrderService
{
    public OrderService() { }
}

// 5. IEmailNotifier interface
public interface IEmailNotifier
{
}

// 6. EmailNotifier implementing IEmailNotifier
public class EmailNotifier : IEmailNotifier
{
    public EmailNotifier() { }
}

// 7. OrderSettings class
public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

// 8. ServiceCollectionExtensions static class
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