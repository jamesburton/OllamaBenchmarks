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
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true))
        {
            throw new ArgumentException(validationResults[0].ErrorMessage);
        }

        // Assuming the order is confirmed and returned as-is for simplicity
        return request;
    }
}