using System.ComponentModel.DataAnnotations;

namespace OrderService
{
    public class CreateOrderRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderService
    {
        public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(request);
            Validator.TryValidateObject(request, validationResults, context, validateAllProperties: true);

            if (validationResults.Count > 0)
            {
                throw new ArgumentException(string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            return request;
        }
    }
}