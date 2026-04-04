using System.ComponentModel.DataAnnotations;

class CreateOrderRequest
{
    [Required]
    public string Name { get; set; }

    [Range(0.01m, 10000m)]
    public decimal Price { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }
}

class OrderService
{
    public object ValidateAndCreate(CreateOrderRequest request)
    {
        var validationResults = Validator.TryValidateObject(request, true, null, validateAllProperties: true);
        if (!validationResults.Success)
        {
            var firstError = (ValidatorValidationError)validationResults?.FirstOrDefault();
            throw new ArgumentException(firstError?.Message ?? "Validation failed", firstError?.MemberName);
        }
        return request;
    }
}