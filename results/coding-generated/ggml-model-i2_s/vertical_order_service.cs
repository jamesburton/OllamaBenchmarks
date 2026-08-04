using OneOf;
using Xunit;
using AwesomeAssertions;
using NSubstitute;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        var customerName = request.CustomerName;
        var items = request.Items;

        if (customerName == null || customerName == "")
        {
            return ValidationError.CreateMessage("Customer name is required");
        }

        if (items == null || items.Count == 0)
        {
            return ValidationError.CreateMessage("At least one item is required");
        }

        var order = new Order
        {
            Id = items.Sum(item => item.Id) + 1,
            CustomerName = customerName,
            Items = items,
            Total = items.Sum(item => item.Quantity * item.UnitPrice),
            CreatedAt = DateTime.UtcNow
        };

        return order;
    }
}

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