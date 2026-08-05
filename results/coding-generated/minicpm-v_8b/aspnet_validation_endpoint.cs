using System;
using System.ComponentModel.DataAnnotations;

namespace CreateOrderRequest
{
    public class CreateOrderRequest
    {
        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; }

        [Required]
        public string Name { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace OrderService
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.Price)
                .Must(p => p >= 0.01 && p <= 10000);

            RuleFor(x => x.Quantity)
                .Must(p => p >= 1 && p <= 999);

            RuleFor(x => x.Name)
                .Required();
        }
    }

    public class OrderService
    {
        private readonly CreateOrderRequestValidator _validator;

        public OrderService()
        {
            _validator = new CreateOrderRequestValidator();
        }

        public void ValidateAndCreate(CreateOrderRequest request)
        {
            if (!_validator.Validate(request))
            {
                throw new ArgumentException($"Validation failed: {string.Join(", ", _validator.Errors)}");
            }

            // Return the confirmed order
        }
    }
}