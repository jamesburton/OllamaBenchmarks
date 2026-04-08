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
        if (string.IsNullOrEmpty(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            CreatedAt = DateTime.UtcNow
        };

        order.Total = order.Items.Sum(item => item.Quantity * item.UnitPrice);

        _repository.Save(order);

        return order;
    }
}