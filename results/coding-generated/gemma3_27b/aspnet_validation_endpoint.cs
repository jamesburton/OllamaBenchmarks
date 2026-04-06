using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }

    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000.")]
    public decimal Price { get; set; }

    [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999.")]
    public int Quantity { get; set; }
}

public class OrderService
{
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
    {
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true);

        if (!isValid)
        {
            var firstError = validationResults.FirstOrDefault();
            if (firstError != null)
            {
                throw new ArgumentException(firstError.ErrorMessage);
            }
            else
            {
                throw new ArgumentException("Validation failed.");
            }
        }

        return request;
    }
}