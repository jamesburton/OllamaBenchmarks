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
        if (request == null || string.IsNullOrWhiteSpace(request.CustomerName))
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
            CreatedAt = System.DateTime.UtcNow
        };

        decimal total = 0m;
        foreach (var item in order.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        order.Total = total;

        _repository.Save(order);
        return order;
    }
}