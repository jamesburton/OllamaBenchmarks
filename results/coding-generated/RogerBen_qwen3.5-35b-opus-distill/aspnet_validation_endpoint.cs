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
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
        {
            var firstError = validationResults.FirstOrDefault();
            if (firstError != null)
            {
                throw new ArgumentException($"Validation failed: {firstError.ErrorMessage}");
            }
            throw new ArgumentException("Validation failed without specific error message.");
        }

        return request;
    }
}