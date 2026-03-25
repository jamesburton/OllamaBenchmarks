using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register the OrderService for DI (scoped per request)
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

app.MapOrderEndpoints();

app.Run();

// ---------------------------
// Domain model
public record CreateOrderRequest(
    [Required] string Name,
    [Range(0.01, 10000)] decimal Price,
    [Range(1, 999)] int Quantity
);

// Service layer
public class OrderService
{
    public CreateOrderRequest CreateOrder(CreateOrderRequest request)
    {
        // In a real implementation you would persist the order.
        // Here we simply return the validated request as a confirmed order.
        return request;
    }
}

// Endpoint mapping helper
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        // Minimal API automatically validates the request based on data annotations.
        app.MapPost("/api/orders", (CreateOrderRequest request, OrderService service) =>
        {
            var order = service.CreateOrder(request);
            return Results.Ok(order);
        })
        .Accepts<CreateOrderRequest>("application/json")
        .Produces<CreateOrderRequest>(200);
    }
}