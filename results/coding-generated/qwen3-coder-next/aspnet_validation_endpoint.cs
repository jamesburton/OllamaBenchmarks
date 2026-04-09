using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string? Name { get; set; }

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var context = new ValidationContext(request) { MemberName = "request" };
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true))
        {
            var firstError = validationResults.FirstOrDefault();
            throw new ArgumentException(firstError?.ErrorMessage ?? "Validation failed");
        }

        return request;
    }
}