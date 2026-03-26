using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0.01m, 10000)]
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
    public static IEndpointRouteBuilder MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/api/orders", async (CreateOrderRequest request, OrderService orderService) =>
        {
            var validationContext = new ValidationContext(request) { MemberName = string.Empty };
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            if (!isValid)
            {
                return Results.ValidationProblem(validationResults.ToDictionary(
                    r => r.MemberNames.FirstOrDefault() ?? string.Empty,
                    r => r.ErrorMessage));
            }

            var order = orderService.CreateOrder(request);
            return Results.Ok(order);
        });

        return app;
    }
}