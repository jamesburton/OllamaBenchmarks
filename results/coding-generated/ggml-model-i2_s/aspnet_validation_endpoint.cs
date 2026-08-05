using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateOrderRequestService
{
    public class CreateOrderRequest
    {
        [Required]
        [Range(0.01, 10000)]
        [Decimal]
        public decimal Price { get; set; }

        [Range(1, 999)]
        [Min(1, 1)]
        [Max(999, 999)]
        public int Quantity { get; set; }
    }

    public class CreateOrderRequestValidator : Validator
    {
        public bool Validate(object value)
        {
            if (value is CreateOrderRequest)
            {
                if (!string.IsNullOrEmpty(value.Name))
                {
                    return value.Name.Length > 0;
                }
                return value.Price >= 0;
            }
            return false;
        }
    }

    public class OrderService
    {
        public async Task ValidateAndCreate(CreateOrderRequest request)
        {
            try
            {
                var validator = new CreateOrderRequestValidator();
                await validator.ValidateAsync(request);
                return request;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Validation error: {ex.Message}");
                throw;
            }
        }
    }
}