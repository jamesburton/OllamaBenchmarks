using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using OneOf;

[Owned]
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
    public DateTime CreatedAt { get; set; }
}

public class CreateOrderRequest
{
    [Required]
    public string CustomerName { get; set; }

    [Required]
    public List<OrderItem> Items { get; set; }
}

public record ValidationError(string Message);

public interface IOrderService
{
    OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request);
}

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public OrderDbContext(DbContextOptions options) : base(options)
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
            return new ValidationError("Customer name is required.");

        if (request.Items == null || request.Items.Count == 0)
            return new ValidationError("At least one order item is required.");

        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = CalculateTotal(request.Items),
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        return order;
    }

    private decimal CalculateTotal(List<OrderItem> items)
    {
        decimal total = 0m;
        foreach (var item in items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        return total;
    }
}

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
        return request.Match<IActionResult>(
            valid => _orderService.CreateOrder(valid).Match(
                order => CreatedAtAction(nameof(CreateOrder), new { id = order.Id }, order),
                error => BadRequest(error.Message)),
            _ => BadRequest("Invalid request"));
    }
}

public class OrderTests
{
    [Fact]
    public void CreateOrder_HappyPath()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new OrderDbContext(options);
        var service = new OrderService(context);

        var request = new CreateOrderRequest
        {
            CustomerName = "Alice",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "X", Quantity = 1, UnitPrice = 10m }
            }
        };

        var result = service.CreateOrder(request).Match(
            order => order,
            error => throw new Exception("Validation failed"));

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Total.Should().Be(10m);
    }

    [Fact]
    public void CreateOrder_ValidationError_EmptyCustomerName()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new OrderDbContext(options);
        var service = new OrderService(context);

        var request = new CreateOrderRequest
        {
            CustomerName = "",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "X", Quantity = 1, UnitPrice = 10m }
            }
        };

        var result = service.CreateOrder(request).Match(
            _ => throw new Exception("Expected validation error"),
            error => error);

        error.Message.Should().Be("Customer name is required.");
    }

    [Fact]
    public void OrdersController_CreateOrder_Success()
    {
        var mockService = Substitute.For<IOrderService>();
        var controller = new OrdersController(mockService);

        var request = new CreateOrderRequest
        {
            CustomerName = "Alice",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "X", Quantity = 1, UnitPrice = 10m }
            }
        };

        var order = new Order { Id = 1 };
        mockService.CreateOrder(request).Returns(order);

        var result = controller.CreateOrder(request) as CreatedAtActionResult;

        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(order);
    }

    [Fact]
    public void OrdersController_CreateOrder_ValidationError()
    {
        var mockService = Substitute.For<IOrderService>();
        var controller = new OrdersController(mockService);

        var request = new CreateOrderRequest
        {
            CustomerName = "",
            Items = new List<OrderItem>
        };

        var error = new ValidationError("Customer name is required.");
        mockService.CreateOrder(request).Returns(error);

        var result = controller.CreateOrder(request) as BadRequestObjectResult;

        result.Should().NotBeNull();
        ((ValidationError)((ObjectResult)result.Value).Value).Message.Should().Be("Customer name is required.");
    }
}