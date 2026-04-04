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
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
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
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) => _repo = repo;

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName))
            return OneOf.FromT0(new ValidationError { Message = "Customer name is required" });

        if (request.Items == null || !request.Items.Any())
            return OneOf.FromT0(new ValidationError { Message = "At least one item is required" });

        var order = new Order
        {
            Id = 0,
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = request.Items.Sum(item => item.Quantity * item.UnitPrice),
            CreatedAt = DateTime.UtcNow
        };
        _repo.Save(order);
        return OneOf.FromT0(order);
    }
}