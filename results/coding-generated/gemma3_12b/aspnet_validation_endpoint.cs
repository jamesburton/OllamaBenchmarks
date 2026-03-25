using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

public record CreateOrderRequest(
    [Required] string Name,
    [Range(0.01, 10000)] decimal Price,
    [Range(1, 999)] int Quantity
);

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
        app.MapPost("/api/orders", async (CreateOrderRequest request, OrderService orderService) =>
        {
            try
            {
                // Validation is handled by the record attributes.  No explicit validation needed here.
                var order = orderService.CreateOrder(request);
                return Results.Ok(order);
            }
            catch (Exception ex)
            {
                // Handle validation errors or other exceptions.
                return Results.Problem(detail: ex.Message, statusCode: 400);
            }
        });
    }
}