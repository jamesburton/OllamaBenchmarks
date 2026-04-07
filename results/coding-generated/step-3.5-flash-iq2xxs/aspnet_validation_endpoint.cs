using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderService
{
    public object ValidateAndCreate(CreateOrderRequest request)
    {
        var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request, null, null);
        bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, validateAllProperties: true, results: results);
        if (!isvalid)
        {
            var firstError = results.FirstOrDefault();
            throw new System.ArgumentException(firstError?.ErrorMessage ?? "Validation failed");
        }
        return request;
    }
}

try
{
    Validator.ValidateObject(request, new ValidationContext(request, null, null), validateAllProperties: true);
    return request;
}
catch (ValidationException ex)
{
    throw new ArgumentException(ex.Message);
}