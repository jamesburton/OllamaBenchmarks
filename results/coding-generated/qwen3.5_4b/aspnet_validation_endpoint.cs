using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
            {
                [Required]
                public string Name { get; set; }

                [Range(0.01m, 10000m)] // Note: Range expects double or decimal. 0.01m is decimal.
                public decimal Price { get; set; }

                [Range(1, 999)]
                public int Quantity { get; set; }
            }

public class OrderService
            {
                public OrderResult ValidateAndCreate(CreateOrderRequest request)
                {
                    var errors = new List<ValidationResult>();
                    bool isValid = Validator.TryValidateObject(request, errors, true);

                    if (!isValid)
                    {
                        // Get first error
                        var firstError = errors[0];
                        throw new ArgumentException($"Validation failed: {firstError.ErrorMessage}", nameof(request));
                    }

                    return request;
                }
            }