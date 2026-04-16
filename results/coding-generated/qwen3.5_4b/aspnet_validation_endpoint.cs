using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01m, 10000m)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public Order CreateOrder(CreateOrderRequest request)
    {
        var validationResult = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            null,
            validateAllProperties: true
        );

        if (validationResult.Errors.Count > 0)
        {
            var firstError = validationResult.Errors.First();
            throw new ArgumentException($"Validation failed: {firstError.ErrorMessage}");
        }

        return request;
    }
}