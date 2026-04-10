using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        // Validate the request using Validator.TryValidateObject
        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);

        if (!isValid)
        {
            // If validation fails, throw ArgumentException with details of the first validation error
            var firstError = validationResults.FirstOrDefault();
            if (firstError != null)
            {
                throw new ArgumentException($"Validation failed: {firstError.ErrorMessage}", firstError);
            }
            // Fallback, though theoretically covered by !isValid
            throw new ArgumentException("Validation failed for unknown reasons.");
        }

        // If validation passes, return the request as a confirmed order
        return request;
    }
}