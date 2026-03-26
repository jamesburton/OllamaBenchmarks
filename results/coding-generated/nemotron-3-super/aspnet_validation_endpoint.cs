using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; } = default!;

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationResults = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = Validator.TryValidateObject(
            request,
            new System.ComponentModel.DataAnnotations.ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            var firstError = validationResults[0];
            string errorMsg = firstError.ErrorMessage ?? "Validation failed";
            throw new System.ArgumentException(errorMsg, nameof(request));
        }

        return request;
    }
}