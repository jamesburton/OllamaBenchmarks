using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01m, 10000.00m)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);
        if (!isValid)
        {
            var firstError = results.First();
            throw new ArgumentException(firstError.ErrorMessage);
        }
        return request;
    }
}