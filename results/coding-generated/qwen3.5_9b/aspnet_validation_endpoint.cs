using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Range(0.01m, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        if (!Validator.TryValidateObject(request, new ValidationContext(request), out var validationErrors))
        {
            throw new ArgumentException($"Validation failed: {validationErrors[0].ErrorMessage}", nameof(request));
        }

        return request;
    }
}