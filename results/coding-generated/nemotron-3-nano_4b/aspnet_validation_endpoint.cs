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
        var validator = new System.ComponentModel.DataAnnotations.Validator<CreateOrderRequest>(request, null, null, false);
        if (!validator.TryValidateObject(request, true, null, true))
        {
            var invalid = null;
            foreach (var attr in validator.ValidationState.Instances.SelectMany(s => s.Errors))
            {
                invalid = attr;
                break;
            }
            throw new ArgumentException(invalid.ErrorMessage);
        }
        return request;
    }
}