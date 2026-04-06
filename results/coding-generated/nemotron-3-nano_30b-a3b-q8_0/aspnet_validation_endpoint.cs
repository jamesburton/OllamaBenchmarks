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
        foreach (var prop in typeof(CreateOrderRequest).GetProperties())
        {
            var result = new ValidationResult();
            if (!Validator.TryValidateProperty(request, prop.Name, validationContext, out result))
            {
                var errorMsg = $"{prop.Name}: {result.ErrorMessage}";
                throw new ArgumentException(errorMsg);
            }
        }
        return request;
    }
}