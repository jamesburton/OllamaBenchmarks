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
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, validationContext, results, validateAllProperties: true);

        if (results.Count == 0)
            return request;

        var firstError = results[0];
        throw new ArgumentException(string.Format("Validation error: {0} Property: {1}", firstError.ErrorMessage, firstError.PropertyName));
    }
}