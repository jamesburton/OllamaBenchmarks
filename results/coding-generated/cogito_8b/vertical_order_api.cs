public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Order()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void CalculateTotal()
    {
        Total = Items.Sum(i => i.Quantity * i.UnitPrice);
    }
}

[Owned]
public class OrderItem
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public OrderItem()
    {
        Quantity = 1;
        UnitPrice = 0m;
    }
}

[Required]
public class CreateOrderRequest
{
    [Required]
    public string CustomerName { get; set; }

    [Required]
    public List<OrderItem> Items { get; set; } = new();
}

public record ValidationError(string Message);

public interface IOrderService
{
    OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request);
}

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }
}

public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;

    public OrderService(OrderDbContext context)
    {
        _context = context;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return new ValidationError("Customer name is required");

        if (!request.Items.Any())
            return new ValidationError("At least one order item is required");

        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items.ToList()
        };

        order.CalculateTotal();
        _context.Orders.Add(order);
        _context.SaveChanges();

        return order;
    }
}

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateOrderRequest request)
    {
        var result = _orderService.CreateOrder(request);

        return result.Match<IActionResult>(
            order => CreatedAtAction(nameof(OrdersController), new { id = order.Id }, order),
            error => BadRequest(error.Message)
        );
    }
}

public class OrderTests
{
    [Fact]
    public async Task CreateOrder_HappyPath_ReturnsCreated()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        using (var context = new OrderDbContext(dbContextOptions))
        {
            var service = new OrderService(context);

            var request = new CreateOrderRequest
            {
                CustomerName = "Alice",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Product 1", Quantity = 2, UnitPrice = 10m }
                }
            };

            // Act
            var result = service.CreateOrder(request);

            // Assert
            result.Match(
                order => Assert.Equal(201, StatusCode),
                error => Assert.Fail($"Unexpected error: {error.Message}")
            );
        }
    }

    [Fact]
    public async Task CreateOrder_ValidationErrors_ReturnsBadRequest()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        using (var context = new OrderDbContext(dbContextOptions))
        {
            var service = new OrderService(context);

            var request = new CreateOrderRequest
            {
                CustomerName = "",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Product 1", Quantity = 2, UnitPrice = 10m }
                }
            };

            // Act
            var result = service.CreateOrder(request);

            // Assert
            result.Match(
                order => Assert.Fail("Unexpected success"),
                error => Assert.Equal(400, StatusCode)
            );
        }
    }

    [Fact]
    public async Task CreateOrderController_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var mockService = Substitute.For<IOrderService>();

        mockService.CreateOrder(Arg.Any<CreateOrderRequest>())
            .Returns(new Order { Id = 1 });

        var controller = new OrdersController(mockService);

        // Act
        var result = await controller.Create(
            new CreateOrderRequest
            {
                CustomerName = "Alice",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Product 1", Quantity = 2, UnitPrice = 10m }
                }
            });

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateOrderController_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var mockService = Substitute.For<IOrderService>();

        mockService.CreateOrder(Arg.Any<CreateOrderRequest>())
            .Returns(new ValidationError("Customer name is required"));

        var controller = new OrdersController(mockService);

        // Act
        var result = await controller.Create(
            new CreateOrderRequest
            {
                CustomerName = "",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Product 1", Quantity = 2, UnitPrice = 10m }
                }
            });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}