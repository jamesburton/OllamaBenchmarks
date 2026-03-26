using System.ComponentModel.DataAnnotations;

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
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request, null, null);
        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            var errors = validationResults.Select(r => r.ErrorMessage).FirstOrDefault() ?? "Unknown validation error";
            throw new ArgumentException($"Validation failed: {errors}");
        }
        return request;
    }
}