using System.ComponentModel.DataAnnotations;

[Required]
public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Range(0.01m, 10000)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        if (!Validator.TryValidateObject(request, new ValidationContext(request), out var errors))
        {
            throw new ArgumentException(errors[0].ErrorMessage);
        }

        return request;
    }
}