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
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var validationResults = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
        bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, validationContext, validationResults, true);
        if (!isValid)
        {
            System.ComponentModel.DataAnnotations.ValidationResult? firstError = null;
            foreach (var result in validationResults)
            {
                firstError = result;
                break;
            }
            var errorMessage = firstError?.ErrorMessage ?? "Invalid request.";
            throw new System.ArgumentException($"Validation failed: {errorMessage}", nameof(request));
        }
        return request;
    }
}