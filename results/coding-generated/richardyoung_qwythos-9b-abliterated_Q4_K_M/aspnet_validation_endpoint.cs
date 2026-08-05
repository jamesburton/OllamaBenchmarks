using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest {
    [Required] public string Name { get; set; }
    [Range(0.01m, 10000m)] public decimal Price { get; set; }
    [Range(1, 999)] public int Quantity { get; set; }
}

public class OrderService {
    public CreateOrderRequest ValidateAndCreate(CreateOrderRequest request) {
        Validator.TryValidateObject(request, new ValidationContext(request), out var errors);
        if (errors.Count > 0) throw new ArgumentException(errors[0].ErrorMessage);
        return request;
    }
}