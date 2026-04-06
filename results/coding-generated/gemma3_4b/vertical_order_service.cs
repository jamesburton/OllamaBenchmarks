using OneOf;
using AwesomeAssertions;
using NSubstitute;
using xunit;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
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

        decimal total = 0;
        foreach (var item in order.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }

        order.Total = total;

        _orderRepository.Save(order);

        return order;
    }
}

public class OrderItem
{
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ValidationError
{
    public string Message { get; set; } = "";
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = "";
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
}