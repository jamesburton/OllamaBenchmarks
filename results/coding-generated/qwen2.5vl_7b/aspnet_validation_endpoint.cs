using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        app.MapPost("/api/orders", (CreateOrderRequest request) =>
        {
            var orderService = app.Services.GetService<OrderService>();
            var order = orderService.CreateOrder(request);
            return Results.Ok(order);
        });
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddScoped<OrderService>();
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapOrderEndpoints();

        app.Run();
    }
}