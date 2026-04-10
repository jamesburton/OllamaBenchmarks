public interface IOrderRepository { }

OrderRepository
public class OrderRepository : IOrderRepository { }

IOrderService
public interface IOrderService { }

OrderService
public class OrderService : IOrderService { }

IEmailNotifier
public interface IEmailNotifier { }

EmailNotifier
public class EmailNotifier : IEmailNotifier { }

OrderSettings
public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

ServiceCollectionExtensions
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