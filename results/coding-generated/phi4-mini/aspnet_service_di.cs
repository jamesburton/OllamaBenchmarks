public interface IOrderRepository { }
public class OrderRepository : IOrderRepository {
    public OrderRepository() {}
}
public interface IOrderService { }
public class OrderService : IOrderService {
    public OrderService() {}
}
public interface IEmailNotifier { }
public class EmailNotifier : IEmailNotifier {
    public EmailNotifier() {}
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
        services.Configure<OrderSettings>(options => {
            var warehouseCode = options.GetSection("Orders:WarehouseCode").Value;
            var maxRetries = int.Parse(options.GetSection("Orders:MaxRetries").Value);
            ((IOptions<OrderSettings>)services).Set(new OrderSettings { WarehouseCode = warehouseCode, MaxRetries = maxRetries });
        });

        services.AddTransient<IEmailNotifier, EmailNotifier>();

        return services;
    }
}