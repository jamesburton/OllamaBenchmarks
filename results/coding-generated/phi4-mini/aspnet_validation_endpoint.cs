using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public interface IOrderService
{
    CreateOrderRequest ValidateAndCreate(CreateOrderRequest request);
}

public class OrderService : IOrderService
{
    private readonly Validator _validator;

    public OrderService(IValidator validator)
    {
        _validator = validator;
    }

    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationResults = new List<ValidationResult>();
        bool isValid = _validator.TryValidateObject(request, validationResults, true);

        if (!isValid && !validationResults.Any())
        {
            throw new ArgumentException("Invalid order details provided.");
        }

        return request;
    }
}

public class Validator : IValidator
{
    public void Validate(object value, ValidationContext context)
    {
        // No need to implement anything for this example as we're using System.ComponentModel.DataAnnotations directly.
    }
}