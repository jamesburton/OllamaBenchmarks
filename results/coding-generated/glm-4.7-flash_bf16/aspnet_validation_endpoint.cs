using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 10000.00, ErrorMessage = "Price must be greater than 0 and less than or equal to 10,000.00")]
    public decimal Price { get; set; }

    [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999")]
    public int Quantity { get; set; }
}

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderService
{
    public Order ValidateAndCreate(CreateOrderRequest request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request, context, results, true))
        {
            throw new ArgumentException("Validation failed", nameof(request));
        }

        return new Order
        {
            Name = request.Name,
            Price = request.Price,
            Quantity = request.Quantity
        };
    }
}