using OneOf;

public class OrderItem
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; }
}

public class ValidationError
{
    public string Message { get; set; }
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetById(int id);
}

public class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        // 1. Validate CustomerName
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return OneOf<Order, ValidationError>.FromT1(new ValidationError { Message = "Customer name is required" });
        }

        // 2. Validate Items
        if (request.Items == null || !request.Items.Any())
        {
            return OneOf<Order, ValidationError>.FromT1(new ValidationError { Message = "At least one item is required" });
        }

        // 3. Calculate Total
        decimal total = 0;
        foreach (var item in request.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }

        // 4. Create Order
        var newOrder = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = total,
            CreatedAt = DateTime.UtcNow
        };

        // 5. Save
        _repository.Save(newOrder);

        // 6. Return success
        return OneOf<Order, ValidationError>.FromT0(newOrder);
    }
}