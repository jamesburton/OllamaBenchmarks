using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationContext = new ValidationContext(request);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(request, validationContext, results, validateAllProperties: true);

        if (!isValid)
        {
            string firstErrorMessage = results[0].ErrorMessage;
            throw new ArgumentException($"Validation failed: {firstErrorMessage}");
        }

        // If validation passes, return the request as a confirmed order
        return request;
    }
}