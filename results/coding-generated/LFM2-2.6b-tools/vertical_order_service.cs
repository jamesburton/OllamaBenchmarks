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
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; } = new();
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
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return new ValidationError("Customer name is required");

        if (request.Items == null || !request.Items.Any())
            return new ValidationError("At least one item is required");

        var order = new Order();
        foreach (var item in request.Items)
            order.Items.Add(item);

        order.Total = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        _repository.Save(order);
        return order;
    }
}

public class OrderServiceTests
{
    private readonly IOrderRepository _repository;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _repository = new MockRepository<IOrderRepository>();
        _service = new OrderService(_repository);
    }

    [Fact]
    public void CreateOrder_ValidRequest_ReturnsOrder()
    {
        var request = new CreateOrderRequest()
        {
            CustomerName = "John Doe",
            Items = new List<OrderItem> { new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10.0m } }
        };

        var order = _service.CreateOrder(request);

        Assert.IsType<Order>(order);
        Assert.Equal(20.0m, order.Total);
        _repository.Verify(r => r.Save(order), Times.Once());
    }

    [Fact]
    public void CreateOrder_CustomerNameNull_ReturnsValidationError()
    {
        var request = new CreateOrderRequest()
        {
            CustomerName = null,
            Items = new List<OrderItem> { new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10.0m } }
        };

        var error = _service.CreateOrder(request);
        Assert.IsType<ValidationError>(error);
        Assert.Equal("Customer name is required", error.Message);
    }

    [Fact]
    public void CreateOrder_EmptyItemsList_ReturnsValidationError()
    {
        var request = new CreateOrderRequest()
        {
            CustomerName = "Jane Doe",
            Items = new List<OrderItem>()
        };

        var error = _service.CreateOrder(request);
        Assert.IsType<ValidationError>(error);
        Assert.Equal("At least one item is required", error.Message);
    }
}