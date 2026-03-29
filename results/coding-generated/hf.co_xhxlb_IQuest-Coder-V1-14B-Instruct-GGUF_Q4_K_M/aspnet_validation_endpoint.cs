using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

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
        var isValid = Validator.TryValidateObject(request, new ValidationContext(request), validationResults, validateAllProperties: true);
        if (!isValid)
        {
            var firstError = validationResults.First();
            throw new ArgumentException(firstError.ErrorMessage, nameof(request));
        }
        return request;
    }
}