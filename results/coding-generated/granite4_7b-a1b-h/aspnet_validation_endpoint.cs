using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public record CreateOrderRequest(string Name, decimal Price, int Quantity);

public class OrderService
{
    public CreateOrderRequest CreateOrder(CreateOrderRequest request)
    {
        // For simplicity, just return the request as confirmed order
        return request;
    }
}

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/api/orders", async (CreateOrderRequest request, IApplicationBuilder appBuilder) =>
        {
            var orderService = appBuilder.ApplicationServices.GetRequiredService<OrderService>();
            var confirmedOrder = orderService.CreateOrder(request);

            // Return the confirmed order
            return Results.Ok(confirmedOrder);
        });
    }
}