using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;

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
    /// Validates the request and simulates order creation.
    /// </summary>
    /// <param name="request">The order request data.</param>
    /// <returns>The validated request object.</returns>
    /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        // Validate all properties
        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);

        if (!isValid)
        {
            // Throw ArgumentException with details of the first validation error
            var firstError = validationResults[0].ErrorMessage;
            throw new ArgumentException($"Validation failed: {firstError}", nameof(request));
        }

        // Validation passed
        return request;
    }
}

// Example usage (optional, for testing purposes, but not required in the final output structure)
/*
public class Program
{
    public static void Main()
    {
        var service = new OrderService();

        // --- Test Case 1: Success ---
        try
        {
            var goodRequest = new CreateOrderRequest { Name = "Widget", Price = 9.99m, Quantity = 5 };
            var confirmedOrder = service.ValidateAndCreate(goodRequest);
            Console.WriteLine("Success: Order created for " + confirmedOrder.Name);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Error (Should not happen): " + ex.Message);
        }

        Console.WriteLine("--------------------");

        // --- Test Case 2: Failure (Missing Name) ---
        try
        {
            var badRequest = new CreateOrderRequest { Name = null, Price = 10.00m, Quantity = 1 };
            service.ValidateAndCreate(badRequest);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Failure Caught: " + ex.Message);
        }
    }
}
*/