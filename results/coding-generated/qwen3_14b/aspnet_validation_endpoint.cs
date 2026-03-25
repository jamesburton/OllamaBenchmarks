using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.Results;

public record CreateOrderRequest
{
    [Required]
    public string Name { get; init; }

    [Range(0.01m, 10000m)]
    public decimal Price { get; init; }

    [Range(1, 999)]
    public int Quantity { get; init; }
}

public class OrderService
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
        app.MapPost("/api/orders", (CreateOrderRequest request, OrderService service) =>
        {
            var order = service.CreateOrder(request);
            return Results.Ok(order);
        });
    }
}