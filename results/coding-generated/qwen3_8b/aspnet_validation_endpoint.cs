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
        var context = new ValidationContext(request);
        if (!Validator.TryValidateObject(request, context, results, true))
        {
            if (results.Count > 0)
            {
                var firstError = results[0];
                throw new ArgumentException(firstError.ErrorMessage);
            }
        }
        return request;
    }
}