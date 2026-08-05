using System.ComponentModel validation
public class CreateOrderRequest
{
    [Required] string Name;
    [Range(0.01, 10000)] decimal Price;
    [Range(1, 999)] int Quantity;

    public CreateOrderRequest(string name = null, decimal price = null, int quantity = null)
        => Name = name, Price = price, Quantity = quantity
}