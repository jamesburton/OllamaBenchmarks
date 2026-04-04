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

        var total = request.Items.Sum(item => item.Quantity * item.UnitPrice);

        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = total,
            CreatedAt = DateTime.UtcNow
        };

        _repository.Save(order);

        return order;
    }
}

public class OrderServiceTests
{
    [Fact]
    public void CreateOrder_ValidRequest_ReturnsOrderAndSaves()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest
        {
            CustomerName = "Test Customer",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Item 1", Quantity = 2, UnitPrice = 10.5m },
                new OrderItem { ProductName = "Item 2", Quantity = 1, UnitPrice = 5.0m }
            }
        };

        // Act
        var result = service.CreateOrder(request);

        // Assert
        result.IsT0.Should().BeTrue("Result should be an Order");
        var order = result.AsT0;

        order.Should().NotBeNull();
        order.CustomerName.Should().Be("Test Customer");
        order.Total.Should().Be(26.0m); // (2 * 10.5) + (1 * 5.0)
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        repository.Received().Save(order);
    }

    [Fact]
    public void CreateOrder_MissingCustomerName_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest
        {
            CustomerName = "",
            Items = new List<OrderItem> { new OrderItem { ProductName = "P1", Quantity = 1, UnitPrice = 1 } }
        };

        // Act
        var result = service.CreateOrder(request);

        // Assert
        result.IsT1.Should().BeTrue("Result should be a ValidationError");
        var error = result.AsT1;

        error.Message.Should().Be("Customer name is required");
        repository.DidNotReceive().Save(Arg.Any<Order>());
    }

    [Fact]
    public void CreateOrder_EmptyItems_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest
        {
            CustomerName = "Valid Name",
            Items = new List<OrderItem>()
        };

        // Act
        var result = service.CreateOrder(request);

        // Assert
        result.IsT1.Should().BeTrue("Result should be a ValidationError");
        var error = result.AsT1;

        error.Message.Should().Be("At least one item is required");
        repository.DidNotReceive().Save(Arg.Any<Order>());
    }
}