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
    public object ValidateAndCreate(CreateOrderRequest request)
    {
        var validationContext = new ValidationContext(request, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);

        if (!isValid)
        {
            var firstError = validationResults.FirstOrDefault();
            throw new ArgumentException(firstError?.ErrorMessage ?? "Validation failed");
        }

        return request;
    }
}