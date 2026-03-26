using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        if (!Validator.TryValidateObject(request, new ValidationContext(request), validateAllProperties: true))
        {
            var errors = Validator.GetValidationErrors(request);
            var firstError = errors.FirstOrDefault();
            throw new ArgumentException($"Validation failed: {firstError?.ErrorMessage ?? "Unknown error"}");
        }

        return request;
    }
}