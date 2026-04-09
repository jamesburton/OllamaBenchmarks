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
            Id = 0,
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
    public void CreateOrder_Should_ReturnValidationError_WhenCustomerNameIsEmpty()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest 
        { 
            CustomerName = "", 
            Items = new List<OrderItem> { new OrderItem() } 
        };

        // Act
        var result = service.CreateOrder(request);

        // Assert
        var error = result.Match<ValidationError>(
            order => null,
            err => err
        );

        error.Should().NotBeNull();
        error.Message.Should().Be("Customer name is required");
        repository.DidNotReceive().Save(Arg.Any<Order>());
    }

    [Fact]
    public void CreateOrder_Should_ReturnValidationError_WhenItemsAreEmpty()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var service = new OrderService(repository);
        var request = new CreateOrderRequest 
        { 
            CustomerName = "Test Customer", 
            Items = new List<OrderItem>() 
        };

        // Act
        var result = service.CreateOrder(request);

        // Assert
        var error = result.Match<ValidationError>(
            order => null,
            err => err
        );

        error.Should().NotBeNull();
        error.Message.Should().Be("At least one item is required");
    }

    [Fact]
    public void CreateOrder_Should_CreateOrderAndSave_WhenRequestIsValid()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var service = new OrderService(repository);
        var items = new List<OrderItem>
        {
            new OrderItem { ProductName = "Item 1", Quantity = 2, UnitPrice = 10.0m },
            new OrderItem { ProductName = "Item 2", Quantity = 1, UnitPrice = 5.0m }
        };
        var request = new CreateOrderRequest 
        { 
            CustomerName = "John Doe", 
            Items = items 
        };

        // Act
        var result = service.CreateOrder(request);

        // Assert
        var order = result.Match<Order>(
            o => o,
            err => null
        );

        order.Should().NotBeNull();
        order.CustomerName.Should().Be("John Doe");
        order.Total.Should().Be(25.0m); // (2 * 10) + (1 * 5)
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        repository.Received(1).Save(Arg.Is<Order>(o => 
            o.CustomerName == "John Doe" && 
            o.Total == 25.0m
        ));
    }
}