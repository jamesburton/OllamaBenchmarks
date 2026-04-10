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
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        if (!isValid)
        {
            // Get the first validation error message
            string errorMessage = results.Count > 0 ? results[0].ErrorMessage : "Validation failed.";
            throw new ArgumentException(errorMessage);
        }

        return request;
    }
}