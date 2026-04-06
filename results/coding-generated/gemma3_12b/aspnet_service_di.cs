public interface IOrderRepository { }

// OrderRepository.cs
public class OrderRepository : IOrderRepository {
    public OrderRepository() { }
}

// IOrderService.cs
public interface IOrderService { }

// OrderService.cs
public class OrderService : IOrderService {
    public OrderService() { }
}

// IEmailNotifier.cs
public interface IEmailNotifier { }

// EmailNotifier.cs
public class EmailNotifier : IEmailNotifier {
    public EmailNotifier() { }
}

// OrderSettings.cs
public class OrderSettings {
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

// ServiceCollectionExtensions.cs
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