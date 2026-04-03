using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;

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
    /// <summary>
    /// Validates the incoming request and returns it if valid.
    /// </summary>
    /// <param name="request">The order request data.</param>
    /// <returns>The validated request.</returns>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        // Validate the object
        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);

        if (!isValid)
        {
            // Throw ArgumentException with details of the first validation error
            var firstError = validationResults[0].ErrorMessage;
            throw new ArgumentException($"Validation failed: {firstError}");
        }

        // Validation passed
        return request;
    }
}