using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel;

// 1. CreateOrderRequest class
public class CreateOrderRequest
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }

    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000.")]
    public decimal Price { get; set; }

    [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999.")]
    public int Quantity { get; set; }
}

// 2. OrderService class
public class OrderService
{
    /// <summary>
    /// Validates the incoming request and returns the order if valid.
    /// </summary>
    /// <param name="request">The order request data.</param>
    /// <returns>The validated request object.</returns>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        // Validate the object using the specified method
        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);

        if (!isValid)
        {
            // If validation fails, throw ArgumentException with details of the first error
            var firstError = validationResults[0].ErrorMessage;
            throw new ArgumentException($"Validation failed: {firstError}");
        }

        // If validation passes, return the request as a confirmed order
        return request;
    }
}