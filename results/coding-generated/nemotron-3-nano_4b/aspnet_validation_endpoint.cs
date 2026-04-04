using System.ComponentModel.DataAnnotations;

[Required] public string Name { get; set; }
[Range(0.01, 10000)] public decimal Price { get; set; }
[Range(1, 999)] public int Quantity { get; set; }

public class CreateOrderRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var context = new ValidationContext(request);
        var validateResult = Validator.TryValidateObject(request, context, true, Array.Empty<ValidationEntry>());
        if (!validateResult.Success)
        {
            var firstError = request.ValidationErrors.First();
            throw new ArgumentException(firstError.ErrorMessage);
        }
        return request;
    }
}

// Global namespace, no further types.