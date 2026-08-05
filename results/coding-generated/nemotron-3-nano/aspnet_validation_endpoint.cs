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
    public (CreateOrderRequest request) ValidateAndCreate(CreateOrderRequest request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);
        if (!isValid)
        {
            foreach (var result in validationResults)
            {
                if (result != ValidationResult.Success)
                {
                    throw new ArgumentException(result.ErrorMessage ?? "Invalid request", result.MemberNames.First());
                }
            }
        }
        return (request);
    }
}