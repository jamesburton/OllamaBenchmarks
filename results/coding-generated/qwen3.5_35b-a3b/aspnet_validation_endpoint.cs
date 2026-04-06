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

            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var firstError = validationResults.FirstOrDefault();
                throw new System.ArgumentException(firstError.ErrorMessage);
            }

            return request;
        }
    }

string errorMessage = null;
    foreach (var result in validationResults)
    {
        errorMessage = result.ErrorMessage;
        break;
    }
    throw new System.ArgumentException(errorMessage);

string firstError = null;
    foreach (var result in validationResults)
    {
        firstError = result.ErrorMessage;
        break;
    }
    throw new System.ArgumentException(firstError);