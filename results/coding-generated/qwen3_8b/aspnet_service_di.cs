public interface IOrderRepository { }
public class OrderRepository : IOrderRepository { }
public interface IOrderService { }
public class OrderService : IOrderService { }
public interface IEmailNotifier { }
public class EmailNotifier : IEmailNotifier { }
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