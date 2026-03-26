using System.ComponentModel.DataAnnotations;

class CreateOrderRequest {
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

class OrderService {
    public void ValidateAndCreate(CreateOrderRequest request) {
        var validator = new Validator();
        if (!validator.TryValidateObject(request, validator.ValidateAllProperties)) {
            var error = validator.GetFirstValidationError();
            throw new ArgumentException($"{error.Detail}");
        }
        return request;
    }
}