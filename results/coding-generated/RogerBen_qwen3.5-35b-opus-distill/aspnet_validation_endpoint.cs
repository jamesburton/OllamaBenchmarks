using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

// 1. CreateOrderRequest record with validation attributes
public record CreateOrderRequest
{
    [Required]
    public string Name { get; init; }

    [Range(0.01, 10000)]
    public decimal Price { get; init; }

    [Range(1, 999)]
    public int Quantity { get; init; }
}

// 2. OrderService class
public class OrderService
{
    public CreateOrderRequest CreateOrder(CreateOrderRequest request)
    {
        // Return the request as a confirmed order (simplified)
        return request;
    }
}

// 3. OrderEndpoints static class
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/api/orders", (CreateOrderRequest request, OrderService orderService) =>
        {
            var order = orderService.CreateOrder(request);
            return Results.Ok(order);
        });
    }
}