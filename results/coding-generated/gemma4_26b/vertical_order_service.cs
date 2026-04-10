using OneOf;

public class OrderItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class ValidationError
{
    public string Message { get; set; } = string.Empty;
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetById(int id);
}

public class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrder/OrderRepository repository)
    {
        _repository = repository;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
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
            CreatedAt = DateTime.UtcNow,
            Total = request.Items.Sum(item => item.Quantity * item.UnitPrice)
        };

        _repository.Save(order);

        return order;
    }
}

public class OrderServiceTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly OrderService _sut = new OrderService(Substitute.For<IOrderRepository>());

    [Fact]
    public void CreateOrder_WhenCustomerNameIsEmpty_ReturnsValidationError()
    {
        var request = new CreateOrderRequest { CustomerName = "", Items = new List<OrderItem> { new OrderItem() } };

        var result = _sut.CreateOrder(request);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Customer name is required");
    }

    [Fact]
    public void CreateOrder_WhenItemsAreEmpty_ReturnsValidationError()
    {
        var request = new CreateOrderRequest { CustomerName = "John Doe", Items = new List<OrderItem>() };

        var result = _sut.CreateOrder(request);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("At least one item is required");
    }

    [Fact]
    public void CreateOrder_WithValidRequest_SavesOrderAndReturnsOrder()
    {
        var repository = Substitute.For<IOrderRepository>();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest
        {
            CustomerName = "John Doe",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10.0m },
                new OrderItem { ProductName = "Gadget", Quantity = 1, UnitPrice = 5.0m }
            }
        };

        var result = service.CreateOrder(request);

        result.IsT0.Should().BeTrue();
        var order = result.AsT0;
        order.Total.Should().Be(25.0m);
        order.CustomerName.Should().Be("John Doe");

        repository.Received(1).Save(Arg.Is<Order>(o => o.Total == 25.0m));
    }
}