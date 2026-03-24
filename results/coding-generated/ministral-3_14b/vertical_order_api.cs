using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.TypeConstraints;
using Xunit;
using AwesomeAssertions;
using NSubstitute;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Value Objects
public record OrderItem(string ProductName, int Quantity, decimal UnitPrice)
{
    public decimal CalculateTotal() => Quantity * UnitPrice;
}

public record CreateOrderRequest(
    [Required] string CustomerName,
    [Required] List<OrderItem> Items);

// Domain Model
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Error Types
public record ValidationError(string Message);

// Service Interface
public interface IOrderService
{
    OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request);
}

// Database Context
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .OwnsMany(o => o.Items)
            .ToJson();
    }
}

// Service Implementation
public class OrderService : IOrderService
{
    private readonly OrderDbContext _dbContext;

    public OrderService(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return new ValidationError("Customer name is required");

        if (request.Items == null || !request.Items.Any())
            return new ValidationError("At least one order item is required");

        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = request.Items.Sum(i => i.CalculateTotal()),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();

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
            order => CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order),
            error => BadRequest(error.Message)
        );

        IActionResult GetOrder(int id) => Ok(_orderService.GetOrder(id));
    }
}

// Tests
public class OrderManagementTests
{
    [Fact]
    public async Task OrderService_CreateOrder_ShouldReturnOrder()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new OrderDbContext(options);
        context.Database.EnsureCreated();

        var service = new OrderService(context);
        var request = new CreateOrderRequest(
            CustomerName: "Test Customer",
            Items: new List<OrderItem>
            {
                new("Product 1", 2, 10.99m),
                new("Product 2", 1, 5.99m)
            });

        // Act
        var result = service.CreateOrder(request);

        // Assert
        result.Should().BeOfType<Order>();
        result.AsT0.Should().NotBeNull();
        result.AsT0.Total.Should().Be(27.97m);
        result.AsT0.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task OrderService_CreateOrder_ShouldReturnValidationError_WhenCustomerNameIsEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new OrderDbContext(options);
        context.Database.EnsureCreated();

        var service = new OrderService(context);
        var request = new CreateOrderRequest(
            CustomerName: "",
            Items: new List<OrderItem>
            {
                new("Product 1", 2, 10.99m)
            });

        // Act
        var result = service.CreateOrder(request);

        // Assert
        result.Should().BeOfType<ValidationError>();
        result.AsT1.Message.Should().Be("Customer name is required");
    }

    [Fact]
    public void OrdersController_CreateOrder_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var mockService = Substitute.For<IOrderService>();
        var order = new Order { Id = 1, CustomerName = "Test", Total = 10.99m };
        mockService.CreateOrder(Arg.Any<CreateOrderRequest>()).Returns(order);

        var controller = new OrdersController(mockService);
        var request = new CreateOrderRequest(
            CustomerName: "Test Customer",
            Items: new List<OrderItem> { new("Product", 1, 10.99m) });

        // Act
        var result = controller.CreateOrder(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        mockService.Received(1).CreateOrder(Arg.Any<CreateOrderRequest>());
    }

    [Fact]
    public void OrdersController_CreateOrder_ShouldReturnBadRequest_WhenValidationError()
    {
        // Arrange
        var mockService = Substitute.For<IOrderService>();
        var error = new ValidationError("Validation failed");
        mockService.CreateOrder(Arg.Any<CreateOrderRequest>()).Returns(error);

        var controller = new OrdersController(mockService);
        var request = new CreateOrderRequest(
            CustomerName: "",
            Items: new List<OrderItem>());

        // Act
        var result = controller.CreateOrder(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        mockService.Received(1).CreateOrder(Arg.Any<CreateOrderRequest>());
    }
}