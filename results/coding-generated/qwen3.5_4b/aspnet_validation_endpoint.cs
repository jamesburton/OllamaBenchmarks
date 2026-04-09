using System.ComponentModel.DataAnnotations;

    public class CreateOrderRequest {
        [Required]
        public string Name { get; set; }

        [Range(0.01m, 10000)]
        public decimal Price { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; }
    }

    public class OrderService {
        public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request) {
            var context = new ValidationContext(request);
            var errors = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(request, context, errors, validateAllProperties: true);

            if (!isValid) {
                var firstError = errors[0];
                throw new ArgumentException(firstError.ErrorMessage);
            }

            return request;
        }
    }

var context = new ValidationContext(request);
        try {
            Validator.ValidateObject(request, context);
        } catch (ValidationException ex) {
            throw new ArgumentException(ex.Message);
        }