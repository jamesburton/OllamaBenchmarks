using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    /// <summary>
    /// Validates the order request and confirms the order if valid.
    /// </summary>
    /// <param name="request">The order request to validate.</param>
    /// <returns>The validated order request.</returns>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(
            request,
            validationContext,
            validationResults,
            true // Validate all properties
        );

        if (!isValid)
        {
            // If validation fails, extract the first error message
            if (validationResults.Count > 0)
            {
                var firstError = validationResults[0].ErrorMessage;
                throw new ArgumentException($"Validation failed for order request. First error: {firstError}", nameof(request));
            }
            // Fallback if somehow validation failed but no results were captured (should not happen)
            throw new ArgumentException("Validation failed, but no specific error details were provided.");
        }

        // Validation passed
        return request;
    }
}