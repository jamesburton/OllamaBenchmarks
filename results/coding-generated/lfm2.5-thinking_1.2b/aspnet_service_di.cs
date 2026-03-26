public class OrderSettings
{
    public string WarehouseCode { get; set; }
    public int MaxRetries { get; set; }
}

interface IOrderRepository
{
}
interface IOrderService
{
}
interface IEmailNotifier
{
}
class OrderRepository : IOrderRepository
{
}
class OrderService : IOrderService
{
}
class EmailNotifier : IEmailNotifier
{
}
static class ServiceCollectionExtensions
{
    public static void AddOrderServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddOrderRepository(OrderRepository);
        services.AddOrderService(OrderService);
        services.AddEmailNotifier(EmailNotifier);
        services.AddSingleton<IOrderRepository, OrderRepository>();
        services.AddSingleton<IEmailNotifier, EmailNotifier>();
    }
}