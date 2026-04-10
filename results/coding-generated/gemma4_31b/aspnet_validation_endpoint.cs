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
        var context = new ValidationContext(request);
        var results = new System.Collections.Generic.List<ValidationResult>();

        if (!Validator.TryValidateObject(request, context, results, validateAllProperties: true))
        {
            throw new System.ArgumentException(results[0].ErrorMessage);
        }

        return request;
    }
}