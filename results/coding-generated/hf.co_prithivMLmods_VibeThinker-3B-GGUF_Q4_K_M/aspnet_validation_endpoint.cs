using System.ComponentModel validation;

public class CreateOrderRequest
{
    [Required] public string Name { get; set; }
    [Range(0.01, 10000)] public decimal Price { get; set; }
    [Range(1, 999)] public int Quantity { get; set; }
}

public class OrderService
{
    public static bool ValidateAndCreate(CreateOrderRequest request)
    {
        if (!Validator.TryValidateObject(request, new[] { typeof(CreateOrderRequest) }, true))
        {
            var errors = Validator.GetValidationErrors(request);
            var firstError = errors.First();
            throw new ArgumentException($"First validation error: {firstError.Message}");
        }
        return request;
    }
}