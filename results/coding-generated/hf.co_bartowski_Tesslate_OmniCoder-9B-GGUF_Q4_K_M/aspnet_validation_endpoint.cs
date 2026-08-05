using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01m, 10000m)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        if (!Validator.TryValidateObject(request, new ValidationContext(request), null, validateAllProperties: true))
        {
            var errors = new List<string>();
            foreach (var error in request.ValidationErrors)
            {
                errors.Add($"{error.MemberName}: {error.ErrorMessage}");
            }
            throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
        }

        return request;
    }
}

using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
...

using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
...

var context = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, context, validateAllProperties: true))
            {
                var firstError = context.ValidationErrors.FirstOrDefault();
                if (firstError != null)
                {
                    throw new ArgumentException($"Validation failed: {firstError.ErrorMessage}");
                }
            }

public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
{
    var context = new ValidationContext(request);
    if (!Validator.TryValidateObject(request, context, validateAllProperties: true))
    {
        var firstError = context.ValidationErrors.FirstOrDefault();
        if (firstError != null)
        {
            throw new ArgumentException($"Validation failed: {firstError.ErrorMessage}");
        }
    }
    return request;
}

using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01m, 10000m)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var context = new ValidationContext(request);
        if (!Validator.TryValidateObject(request, context, validateAllProperties: true))
        {
            var firstError = context.ValidationErrors.FirstOrDefault();
            if (firstError != null)
            {
                throw new ArgumentException($"Validation failed: {firstError.ErrorMessage}");
            }
        }
        return request;
    }
}