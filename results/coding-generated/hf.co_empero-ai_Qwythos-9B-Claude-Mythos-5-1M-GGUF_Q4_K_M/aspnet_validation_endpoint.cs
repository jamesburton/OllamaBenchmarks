using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required] public string Name { get; set; } = null!;
    [Range(0.01m, 10000m)] public decimal Price { get; set; }
    [Range(1, 999)] public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var result = Validator.TryValidateObject(request, new ValidationContext(request), out _, validateAllProperties: true);
        if (!result)
        {
            var errors = ValidationResult.GetErrorMessages();
            throw new ArgumentException($"The field '{errors[0]}' is invalid.");
        }
        return request;
    }
}