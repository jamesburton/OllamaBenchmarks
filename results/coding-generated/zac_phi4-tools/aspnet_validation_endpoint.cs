using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01, 10000)]
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
        app.MapPost("/api/orders", async context =>
        {
            if (!context.Request.HasFormContentType)
            {
                await context.Response.WriteAsJsonAsync(new { error = "Invalid content type" }, status: StatusCodes.Status415UnsupportedMediaType);
                return;
            }

            var request = await context.Request.ReadFromJsonAsync<CreateOrderRequest>();
            if (request == null || !Validator.TryValidateObject(request, new ValidationContext(request), null, true))
            {
                var errors = new System.Collections.Generic.List<string>();

                foreach (var validationError in context.ModelState.Values.SelectMany(v => v.Errors))
                {
                    errors.Add(validationError.ErrorMessage);
                }

                await context.Response.WriteAsJsonAsync(new { error = "Validation failed", details = errors }, status: StatusCodes.Status400BadRequest);
                return;
            }

            var orderService = app.Services.GetRequiredService<OrderService>();
            var confirmedOrder = orderService.CreateOrder(request);

            await context.Response.WriteAsJsonAsync(confirmedOrder, status: StatusCodes.Status200OK);
        });
    }
}

// In your Program.cs or Startup.cs
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddScoped<OrderService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapOrderEndpoints();

        app.Run();
    }
}