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
    public System.Collections.Generic.List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public System.DateTime CreatedAt { get; set; }
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; }
    public System.Collections.Generic.List<OrderItem> Items { get; set; }
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
    public OrderService(IOrderRepository repository) => _repository = repository;

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        var items = request.Items;
        if (items == null || items.Count == 0)
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        decimal total = 0m;
        foreach (var item in items)
        {
            total += item.Quantity * item.UnitPrice;
        }

        var order = new Order
        {
            Id = 0,
            CustomerName = request.CustomerName,
            Items = items,
            Total = total,
            CreatedAt = System.DateTime.UtcNow
        };

        _repository.Save(order);
        return OneOf<Order, ValidationError>.FromT0(order);
    }
}