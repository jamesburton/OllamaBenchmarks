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
    /// Validates the order request and confirms it.
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
            true // validateAllProperties: true
        );

        if (!isValid)
        {
            // If validation fails, throw an ArgumentException with details of the first error.
            if (validationResults.Count > 0)
            {
                var firstError = validationResults[0];
                throw new ArgumentException($"Validation failed for CreateOrderRequest. Error: {firstError.ErrorMessage}", firstError);
            }
            // Fallback, though unlikely if isValid is false and results is empty
            throw new ArgumentException("Validation failed, but no specific error details were found.");
        }

        // If validation passes, return the confirmed order.
        return request;
    }
}