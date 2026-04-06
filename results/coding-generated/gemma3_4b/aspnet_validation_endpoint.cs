using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01m, 10000.0m)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public void ValidateAndCreate(CreateOrderRequest request)
    {
        var context = new ValidationContext(request, null, null);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        if (!isValid)
        {
            throw new ArgumentException("Invalid order request", "Name", results[0].ErrorMessage);
        }
    }
}