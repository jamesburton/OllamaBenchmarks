public interface IOrderRepository { }

// OrderRepository class implementing IOrderRepository
public class OrderRepository : IOrderRepository
{
    public OrderRepository() { }
}

// IOrderService interface
public interface IOrderService { }

// OrderService class implementing IOrderService
public class OrderService : IOrderService
{
    public OrderService() { }
}

// IEmailNotifier interface
public interface IEmailNotifier { }

// EmailNotifier class implementing IEmailNotifier
public class EmailNotifier : IEmailNotifier
{
    public EmailNotifier() { }
}

// OrderSettings class with properties
public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

// ServiceCollectionExtensions static class with extension method
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