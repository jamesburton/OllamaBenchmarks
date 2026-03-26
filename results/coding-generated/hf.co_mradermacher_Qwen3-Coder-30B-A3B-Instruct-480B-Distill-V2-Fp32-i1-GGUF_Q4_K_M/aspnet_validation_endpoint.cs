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
        var validationContext = new ValidationContext(request, null, null);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            var firstError = validationResults.FirstOrDefault();
            throw new ArgumentException(firstError?.ErrorMessage ?? "Validation failed.");
        }

        return request;
    }
}