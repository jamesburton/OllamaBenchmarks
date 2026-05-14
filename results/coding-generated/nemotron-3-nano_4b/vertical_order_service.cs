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
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class CreateOrderRequest
{
    public string CustomerName { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
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
            return OneOf.Order<ValidationError>(new ValidationError
            {
                Message = "Customer name is required"
            });
        }
        if (request.Items?.Count == 0)
        {
            return OneOf.Order<ValidationError>(new ValidationError
            {
                Message = "At least one item is required"
            });
        }
        var order = new Order
        {
            Id = 1,
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = request.Items.Sum(i => i.Quantity * i.UnitPrice),
            CreatedAt = DateTime.UtcNow
        };
        _repository.Save(order);
        return OneOf.Order<Order>(order);
    }
}