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
        var results = new System.Collections.Generic.List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(request, results, true);
        if (!isValid)
        {
            throw new System.ArgumentException(results[0].ErrorMessage);
        }
        return request;
    }
}