using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

// 1. CreateOrderRequest class
public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

// 2. OrderService class
public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationContext = new ValidationContext(request, null, null);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        if (!isValid)
        {
            // If validation fails, throw an ArgumentException with details of the first validation error
            var firstError = validationResults.FirstOrDefault()?.ErrorMessage ?? "Validation failed for unknown reasons.";
            throw new ArgumentException($"Validation failed: {firstError}");
        }

        // If validation passes, return the request as a confirmed order
        return request;
    }
}