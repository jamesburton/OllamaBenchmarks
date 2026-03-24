using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

var services = ServiceCollectionExtensions.AddServices(services);
var app = WebApplication.CreateBuilder(args).Build();

app.MapOrderEndpoints();

app.Run();

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}

public record CreateOrderRequest(
    [Required] string Name,
    [Range(0.01m, 10000m)] decimal Price,
    [Range(1, 999)] int Quantity);

public interface IOrderService
{
    CreateOrderRequest CreateOrder(CreateOrderRequest request);
}

public class OrderService : IOrderService
{
    public CreateOrderRequest CreateOrder(CreateOrderRequest request)
    {
        return request;
    }
}

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/api/orders", ([FromBody] CreateOrderRequest request, IOrderService orderService) =>
            Results.Ok(orderService.CreateOrder(request)));
    }
}