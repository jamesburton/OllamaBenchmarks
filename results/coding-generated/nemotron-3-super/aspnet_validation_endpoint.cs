using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; } = default!;

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationResults = new System.Collections.Generic.List<ValidationResult>();
        var context = new ValidationContext(request);
        if (!Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true))
        {
            var firstError = validationResults.FirstOrDefault();
            string errorMessage = firstError != null ? firstError.ErrorMessage : "Validation failed";
            throw new System.ArgumentException(errorMessage);
        }
        return request;
    }
}