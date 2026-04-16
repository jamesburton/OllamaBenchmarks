using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Range(0.01m, 10000m)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        if (!Validator.TryValidateObject(request, new ValidationContext(request), out var validationErrors, validateAllProperties: true))
        {
            var firstError = validationErrors.Cast<ValidationResult>().FirstOrDefault()?.ErrorMessage;
            if (string.IsNullOrEmpty(firstError))
            {
                throw new ArgumentException("Validation failed.");
            }

            throw new ArgumentException(firstError);
        }

        return request;
    }
}