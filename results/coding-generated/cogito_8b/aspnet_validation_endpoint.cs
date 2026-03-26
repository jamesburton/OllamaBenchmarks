using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01m, 10000m)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
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
        app.MapPost("/api/orders", async (CreateOrderRequest request, OrderService orderService) =>
        {
            if (!Validator.TryValidateObject(request, new ValidationContext(request), null, true))
            {
                return Results.ValidationProblem(new[] { "Invalid request" });
            }

            var order = await orderService.CreateOrder(request);
            return Results.Ok(order);
        });
    }
}