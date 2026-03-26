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
        var validationContext = new ValidationContext(request);
        var validationResults = new System.Collections.Generic.List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);
        if (!isValid)
        {
            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    throw new System.ArgumentException($"{memberName}: {validationResult.ErrorMessage}");
                }
            }
        }
        return request;
    }
}