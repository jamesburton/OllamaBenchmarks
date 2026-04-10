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
        if (request == null)
            throw new System.ArgumentNullException(nameof(request));

        var validationResults = new System.Collections.Generic.List<ValidationResult>();
        var context = new ValidationContext(request, serviceProvider: null, items: null);
        bool isValid = Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true);

        if (!isValid && validationResults.Count > 0)
        {
            var firstError = validationResults[0];
            throw new System.ArgumentException(firstError.ErrorMessage);
        }

        // Validation succeeded – return the confirmed order.
        return request;
    }
}