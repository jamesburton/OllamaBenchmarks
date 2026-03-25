using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using OneOf;
using xunit;
using AwesomeAssertions;

// Order Entity
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

// OrderItem Value Object
public class OrderItem
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// CreateOrderRequest Record
public record CreateOrderRequest([Required] string CustomerName, [Required] List<OrderItem> Items);

// ValidationError Record
public record ValidationError(string Message);

// IOrderService Interface
public interface IOrderService
{
    OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request);
}

// OrderDbContext
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// OrderService Implementation
public class OrderService : IOrderService
{
    private readonly OrderDbContext _dbContext;

    public OrderService(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName))
        {
            return new ValidationError("Customer name is required.");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError("Order items are required.");
        }

        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = request.Items.Sum(item => item.Quantity * item.UnitPrice),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();

        return new OneOf<Order, ValidationError>(order);
    }
}

// OrdersController
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
        OneOf<Order, ValidationError> result = _orderService.CreateOrder(request);
        return result.Match(
            order => CreatedAtAction(nameof(CreateOrder), new { order.Id }, order),
            err => BadRequest(err.Message)
        );
    }
}

// xUnit v3 Tests
public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_HappyPath_InMemory()
    {
        var mockDbContextOptions = new DbContextOptions<OrderDbContext>();
        var dbContext = new OrderDbContext(mockDbContextOptions);

        var orderService = new OrderService(dbContext);

        var request = new CreateOrderRequest(
            CustomerName: "Test Customer",
            Items: new List<OrderItem>
            {
                new OrderItem { ProductName = "Product 1", Quantity = 2, UnitPrice = 10.00m }
            }
        );

        OneOf<Order, ValidationError> result = orderService.CreateOrder(request);

        result.IsT0.ShouldBeTrue();
        var order = (Order)result;
        order.CustomerName.ShouldBe("Test Customer");
        order.Total.ShouldBe(20.00m);
        dbContext.Orders.Count().ShouldBe(1);
    }

    [Fact]
    public async Task CreateOrder_ValidationError_EmptyCustomerName()
    {
        var mockDbContextOptions = new DbContextOptions<OrderDbContext>();
        var dbContext = new OrderDbContext(mockDbContextOptions);

        var orderService = new OrderService(dbContext);

        var request = new CreateOrderRequest(
            CustomerName: "",
            Items: new List<OrderItem>
            {
                new OrderItem { ProductName = "Product 1", Quantity = 2, UnitPrice = 10.00m }
            }
        );

        OneOf<Order, ValidationError> result = orderService.CreateOrder(request);

        result.IsT1.ShouldBeTrue();
        var error = (ValidationError)result;
        error.Message.ShouldBe("Customer name is required.");
    }
}

public class OrdersControllerTests
{
    [Fact]
    public async Task CreateOrder_HappyPath()
    {
        var mockOrderService = Substitute.For<IOrderService>();
        var request = new CreateOrderRequest(
            CustomerName: "Test Customer",
            Items: new List<OrderItem>
            {
                new OrderItem { ProductName = "Product 1", Quantity = 2, UnitPrice = 10.00m }
            }
        );
        var expectedOrder = new Order { Id = 1, CustomerName = "Test Customer", Total = 20.00m };
        mockOrderService.CreateOrder(request).Returns(new OneOf<Order, ValidationError>(expectedOrder));

        var controller = new OrdersController(mockOrderService);
        var result = controller.CreateOrder(request) as CreatedAtActionResult;

        result.ShouldNotBeNull();
        result.ActionName.ShouldBe("CreateOrder");
        result.StatusCode.ShouldBe(201);
        result.Value.ShouldBe(expectedOrder);
    }

    [Fact]
    public async Task CreateOrder_ValidationError()
    {
        var mockOrderService = Substitute.For<IOrderService>();
        var request = new CreateOrderRequest(
            CustomerName: "",
            Items: new List<OrderItem>
            {
                new OrderItem { ProductName = "Product 1", Quantity = 2, UnitPrice = 10.00m }
            }
        );
        var expectedError = new ValidationError("Customer name is required.");
        mockOrderService.CreateOrder(request).Returns(new OneOf<Order, ValidationError>(expectedError));

        var controller = new OrdersController(mockOrderService);
        var result = controller.CreateOrder(request) as BadRequestResult;

        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(400);
    }
}