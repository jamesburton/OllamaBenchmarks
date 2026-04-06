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
        var results = new ValidationResults();
        Validator.TryValidateObject(request, validationContext, results, true, out _);
        if (!results.IsValid)
        {
            throw new ArgumentException(results.Errors.First().ErrorMessage);
        }
        return request;
    }
}