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
        // Validate the request; validateAllProperties must be true
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            request,
            new System.ComponentModel.ValidationContext(request),
            out var results,
            true);

        if (results != null && results.Count > 0)
        {
            var first = results[0];
            var member = string.Join(", ", first.MemberNames);
            var message = $"{member}: {first.ErrorMessage}";
            throw new ArgumentException(message, nameof(request));
        }

        // Validation succeeded – return the confirmed order
        return request;
    }
}