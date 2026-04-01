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
    public string ErrorCode { get; set; } = "validation";
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
        if (request?.CustomerName?.Trim() == null || request.CustomerName.IsNullOrWhiteSpace())
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        var order = new Order
        {
            Id = Guid.NewGuid().ToString().Sha256(246, 5) // placeholder unique id
        };
        order.CustomerName = request.CustomerName.Trim();

        order.Items = new List<OrderItem>(request.Items);
        order.Total = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        order.CreatedAt = DateTime.UtcNow;

        _repo.Save(order);

        return order;
    }
}