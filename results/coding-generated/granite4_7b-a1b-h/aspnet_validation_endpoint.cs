[Required]
public string Name { get; set; }

[Range(0.01m, 10000)]
public decimal Price { get; set; }

[Range(1, 999)]
public int Quantity { get; set; }
}

public class OrderService
{
    public Order CreateOrder(CreateOrderRequest request)
    {
        if (!Validator.TryValidateObject(request, new ValidationContext(request), true))
        {
            throw new ArgumentException(Validator.Message);
        }

        return request;
    }
}