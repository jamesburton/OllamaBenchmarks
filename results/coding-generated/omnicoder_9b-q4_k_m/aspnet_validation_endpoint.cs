public class CreateOrderRequest
        {
            [Required]
            public string Name { get; set; }

            [Range(0.01m, 10000m)]
            public decimal Price { get; set; }

            [Range(1, 999)]
            public int Quantity { get; set; }
        }

public class OrderService
        {
            public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
            {
                var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
                var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
                bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, validateAllProperties: true);

                if (!isValid)
                {
                    if (results.Count > 0)
                    {
                        throw new System.ArgumentException(results[0].ErrorMessage);
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

            [Range(0.01m, 10000m)]
            public decimal Price { get; set; }

            [Range(1, 999)]
            public int Quantity { get; set; }
        }

        public class OrderService
        {
            public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request)
            {
                var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
                var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
                bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, validateAllProperties: true);

                if (!isValid)
                {
                    throw new System.ArgumentException(results[0].ErrorMessage);
                }

                return request;
            }
        }