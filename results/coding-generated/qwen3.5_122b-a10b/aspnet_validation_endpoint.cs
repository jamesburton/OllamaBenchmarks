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
        var results = new System.Collections.Generic.List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(request, validationContext, results, validateAllProperties: true);

        if (!isValid)
        {
            var firstError = results.FirstOrDefault();
            if (firstError != null)
            {
                throw new System.ArgumentException(firstError.ErrorMessage);
            }
            else
            {
                throw new System.ArgumentException("Validation failed");
            }
        }

        return request;
    }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationContext = new ValidationContext(request);
        var results = new System.Collections.Generic.List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(request, validationContext, results, validateAllProperties: true);

        if (!isValid)
        {
            if (results.Count > 0)
            {
                throw new System.ArgumentException(results[0].ErrorMessage);
            }
            else
            {
                throw new System.ArgumentException("Validation failed");
            }
        }

        return request;
    }
}

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
        var results = new System.Collections.Generic.List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(request, validationContext, results, validateAllProperties: true);

        if (!isValid)
        {
            if (results.Count > 0)
            {
                throw new System.ArgumentException(results[0].ErrorMessage);
            }
            else
            {
                throw new System.ArgumentException("Validation failed");
            }
        }

        return request;
    }
}