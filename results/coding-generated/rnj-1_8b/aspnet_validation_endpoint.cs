using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

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
        // In a real application, you would save the order to a database here.
        return request;
    }
}

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var orderService = new OrderService();

        app.MapPost("/api/orders", async (HttpRequest httpRequest) =>
        {
            try
            {
                var request = await httpRequest.ReadFromJsonAsync<CreateOrderRequest>();
                var validationContext = new ValidationContext(request);
                Validator.ValidateObject(request, validationContext, true);

                var order = orderService.CreateOrder(request);
                return Results.Ok(order);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });
    }
}