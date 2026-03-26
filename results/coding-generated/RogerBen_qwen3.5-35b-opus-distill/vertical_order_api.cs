using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Xunit;
using NSubstitute;
using AwesomeAssertions;

// Value Object
public class OrderItem
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// Entity
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Request Record with Validation
public record CreateOrderRequest(
    [Required] string CustomerName,
    [Required] List<OrderItem> Items
);

// Error Record
public record ValidationError(string Message);

// Service Interface
public interface IOrderService
{
    OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request);
}

// DbContext
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }
}

// Service Implementation
public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;

    public OrderService(OrderDbContext context)
    {
        _context = context;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return new ValidationError("Customer name is required");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError("At least one item is required");
        }

        // Create order
        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = request.Items.Sum(i => i.Quantity * i.UnitPrice),
            CreatedAt = DateTime.UtcNow
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
    public IActionResult CreateOrder(CreateOrderRequest request)
    {
        return _orderService.CreateOrder(request).Match<IActionResult>(
            order => CreatedAtRoute("GetOrder", new { id = order.Id }, order),
            error => BadRequest(error.Message)
        );
    }
}

// Tests
public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_HappyPath_ReturnsOrder()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new OrderDbContext(options);
        var service = new OrderService(context);

        var request = new CreateOrderRequest(
            CustomerName: "John Doe",
            Items: new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10.00m }
            }
        );

        // Act
        var result = service.CreateOrder(request);

        // Assert
        result.Match(
            order =>
            {
                order.Should().NotBeNull();
                order.CustomerName.Should().Be("John Doe");
                order.Total.Should().Be(20.00m);
                order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            },
            error => Assert.Fail("Expected order, got validation error")
        );
    }

    [Fact]
    public async Task CreateOrder_ValidationEmptyCustomerName_ReturnsValidationError()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new OrderDbContext(options);
        var service = new OrderService(context);

        var request = new CreateOrderRequest(
            CustomerName: "",
            Items: new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10.00m }
            }
        );

        // Act
        var result = service.CreateOrder(request);

        // Assert
        result.Match(
            order => Assert.Fail("Expected validation error, got order"),
            error => error.Should().NotBeNull()
        );
    }
}

public class OrdersControllerTests
{
    [Fact]
    public async Task CreateOrder_Success_ReturnsCreated()
    {
        // Arrange
        var mockService = Substitute.For<IOrderService>();
        var controller = new OrdersController(mockService);

        var request = new CreateOrderRequest(
            CustomerName: "John Doe",
            Items: new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10.00m }
            }
        );

        var order = new Order { Id = 1, CustomerName = "John Doe", Items = request.Items, Total = 20.00m, CreatedAt = DateTime.UtcNow };
        mockService.CreateOrder(request).Returns(order);

        // Act
        var result = controller.CreateOrder(request);

        // Assert
        result.Should().BeOfType<CreatedAtRouteResult>();
    }

    [Fact]
    public async Task CreateOrder_ValidationError_ReturnsBadRequest()
    {
        // Arrange
        var mockService = Substitute.For<IOrderService>();
        var controller = new OrdersController(mockService);

        var request = new CreateOrderRequest(
            CustomerName: "",
            Items: new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10.00m }
            }
        );

        var error = new ValidationError("Customer name is required");
        mockService.CreateOrder(request).Returns(error);

        // Act
        var result = controller.CreateOrder(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}