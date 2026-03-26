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
        var context = new System.ComponentModel.ValidationContext(request);
        bool isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);
        if (!isValid)
        {
            var firstError = results[0];
            var memberNames = string.Join(", ", firstError.MemberNames);
            throw new ArgumentException($"{memberNames}: {firstError.ErrorMessage}");
        }
        return request;
    }
}