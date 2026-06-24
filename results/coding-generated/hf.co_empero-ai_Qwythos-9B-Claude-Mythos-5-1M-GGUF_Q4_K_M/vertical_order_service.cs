using OneOf;

public class OrderItem
{
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = null!;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = null!;
}

public class ValidationError
{
    public string Message { get; set; } = null!;
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetById(int id);
}

public class OrderService(IOrderRepository repository)
{
    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return new ValidationError { Message = "Customer name is required" };

        if (request.Items == null || request.Items.Count == 0)
            return new ValidationError { Message = "At least one item is required" };

        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = request.Items.Sum(item => item.Quantity * item.UnitPrice),
            CreatedAt = DateTime.UtcNow
        };

        repository.Save(order);
        return order;
    }
}