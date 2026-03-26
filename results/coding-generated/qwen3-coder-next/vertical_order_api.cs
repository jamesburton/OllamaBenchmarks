using OneOf;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

// Entities
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

[Owned]
public class OrderItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// Request DTOs
public class CreateOrderRequest
{
    [Required(ErrorMessage = "Customer name is required")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "At least one order item is required")]
    public List<OrderItem> Items { get; set; } = new();
}

public class ValidationError
{
    public string Message { get; set; } = string.Empty;
}

// Service interface
public interface IOrderService
{
    OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request);
}

// DbContext
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; } = null!;
}

// Service implementation
public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;

    public OrderService(OrderDbContext context)
    {
        _context = context;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "At least one order item is required" };
        }

        // Create order
        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            CreatedAt = DateTime.UtcNow,
            Total = request.Items.Sum(item => item.Quantity * item.UnitPrice)
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        return order;
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = _orderService.CreateOrder(request);
        return result.Match<IActionResult>(
            order => CreatedAtAction(nameof(GetOrderById), "Orders", new { id = order.Id }, order),
            error => BadRequest(error.Message)
        );
    }

    private IActionResult GetOrderById(int id)
    {
        // Placeholder for demonstration - not used in tests
        return NotFound();
    }
}

// xUnit v3 tests
public class OrderServiceTests
{
    private readonly OrderDbContext _context;
    private readonly IOrderService _service;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        _context = new OrderDbContext(options);
        _service = new OrderService(_context);
    }

    [Fact]
    public void CreateOrder_HappyPath_ReturnsOrder()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerName = "Alice",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10m },
                new OrderItem { ProductName = "Gadget", Quantity = 1, UnitPrice = 25m }
            }
        };

        // Act
        var result = _service.CreateOrder(request);

        // Assert
        result.Should().BeAssignableTo<Order>();
        var order = result.AsT0;
        order.Id.Should().BeGreaterThan(0);
        order.CustomerName.Should().Be("Alice");
        order.Items.Should().HaveCount(2);
        order.Total.Should().Be(45m);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateOrder_EmptyCustomerName_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerName = "",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 1, UnitPrice = 10m }
            }
        };

        // Act
        var result = _service.CreateOrder(request);

        // Assert
        result.Should().BeAssignableTo<ValidationError>();
        var error = result.AsT0;
        error.Message.Should().Be("Customer name is required");
    }

    [Fact]
    public void CreateOrder_EmptyItems_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerName = "Bob",
            Items = new List<OrderItem>()
        };

        // Act
        var result = _service.CreateOrder(request);

        // Assert
        result.Should().BeAssignableTo<ValidationError>();
        var error = result.AsT0;
        error.Message.Should().Be("At least one order item is required");
    }
}

public class OrdersControllerTests
{
    private readonly IOrderService _orderService;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _orderService = Substitute.For<IOrderService>();
        _controller = new OrdersController(_orderService);
    }

    [Fact]
    public void CreateOrder_HappyPath_ReturnsCreatedAtResult()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerName = "Alice",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10m }
            }
        };

        var expectedOrder = new Order
        {
            Id = 1,
            CustomerName = "Alice",
            Items = request.Items,
            Total = 20m,
            CreatedAt = DateTime.UtcNow
        };

        _orderService.CreateOrder(request)
            .Returns(OneOf<Order, ValidationError>.FromT0(expectedOrder));

        // Act
        var result = _controller.CreateOrder(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdAtResult = (CreatedAtActionResult)result;
        createdAtResult.Value.Should().Be(expectedOrder);
        createdAtResult.RouteValues["id"].Should().Be(1);
    }

    [Fact]
    public void CreateOrder_ValidationError_ReturnsBadRequestResult()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerName = "",
            Items = new List<OrderItem>()
        };

        var error = new ValidationError { Message = "Customer name is required" };

        _orderService.CreateOrder(request)
            .Returns(OneOf<Order, ValidationError>.FromT1(error));

        // Act
        var result = _controller.CreateOrder(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().Be("Customer name is required");
    }
}