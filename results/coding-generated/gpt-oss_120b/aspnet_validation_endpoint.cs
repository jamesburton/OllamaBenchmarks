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
        var context = new ValidationContext(request);
        var results = new System.Collections.Generic.List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        if (!isValid && results.Count > 0)
        {
            var firstError = results[0];
            throw new System.ArgumentException(firstError.ErrorMessage);
        }

        return request;
    }
}