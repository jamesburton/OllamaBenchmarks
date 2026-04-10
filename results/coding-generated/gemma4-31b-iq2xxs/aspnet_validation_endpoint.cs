public class CreateOrderRequest {
            [Required]
            public string Name { get; set; }
            [Range(0.01, 10000)]
            public decimal Price { get; set; }
            [Range(1, 999)]
            public int Quantity { get; set; }
        }

public class OrderService {
            public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request) {
                var context = own a validator...
                // Validator.TryValidateObject is the key.
            }
        }