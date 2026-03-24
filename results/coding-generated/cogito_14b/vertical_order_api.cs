using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Xunit;
using NSubstitute;
using System.Threading;

#nullable enable

[GenerateOneOf]
public partial record ValidationError(string Message);

[GenerateOneOf]
public partial record CreateOrderRequest(
    [Required] string CustomerName,
    [Required] List<OrderItem> Items
);

public record OrderItem(
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IOrderService
{
    OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request);
}

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
    public DbSet<Order> Orders { get; set; } = null!;
}

public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;

    public OrderService(OrderDbContext context) => _context = context;

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return new ValidationError("Customer name is required");
        if (request.Items == null || !request.Items.Any())
            return new ValidationError("Order must contain items");

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

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service) => _service = service;

    [HttpPost]
    public IActionResult Create(CreateOrderRequest request)
    {
        return _service.CreateOrder(request).Match(
            order => CreatedAtAction(nameof(Create), new { id = order.Id }, order),
            error => BadRequest(error.Message)
        );
    }
}

public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsOrder()
    {
        // Arrange
        var context = new OrderDbContext(new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options);
        var service = new OrderService(context);

        var request = new CreateOrderRequest(
            CustomerName: "John Doe",
            Items: new List<OrderItem>
            {
                new("Laptop", 1, 999.99m),
                new("Mouse", 2, 29.99m)
            }
        );

        // Act
        var result = await service.CreateOrder(request);

        // Assert
        var order = result.AsT0;
        order.Should().NotBeNull();
        order.CustomerName.Should().Be("John Doe");
        order.Items.Should().HaveCount(2);
        order.Total.Should().Be(1059.97m);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateOrder_EmptyCustomerName_ReturnsValidationError()
    {
        // Arrange
        var service = Substitute.For<IOrderService>();

        var request = new CreateOrderRequest(
            CustomerName: "",
            Items: new List<OrderItem> { new("Product", 1, 10m) }
        );

        // Act
        var result = service.CreateOrder(request);

        // Assert
        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Be("Customer name is required");
    }

    [Fact]
    public async Task OrdersController_Create_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var service = Substitute.For<IOrderService>();
        var request = new CreateOrderRequest(
            CustomerName: "Jane Smith",
            Items: new List<OrderItem> { new("Product", 1, 10m) }
        );
        service.CreateOrder(request).Returns(new Order { Id = 1 });

        var controller = new OrdersController(service);

        // Act
        var result = await controller.Create(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeOfType<Order>();
        ((Order)createdResult.Value!).Id.Should().Be(1);
    }
}