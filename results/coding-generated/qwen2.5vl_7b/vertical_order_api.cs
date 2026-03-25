using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Xunit;
using Xunit.Abstractions;

// Order entity with properties: int Id, string CustomerName, List<OrderItem> Items, decimal Total, DateTime CreatedAt
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

// OrderItem value object with properties: string ProductName, int Quantity, decimal UnitPrice
public class OrderItem
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// CreateOrderRequest record with [Required] validation attributes on CustomerName and Items
public class CreateOrderRequest
{
    [Required]
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; }
}

// ValidationError record with a Message string property
public class ValidationError
{
    public string Message { get; set; }
}

// IOrderService interface with method returning OneOf<Order, ValidationError>
public interface IOrderService
{
    OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request);
}

// OrderDbContext : DbContext with DbSet<Order> Orders, using constructor that takes DbContextOptions
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
}

// OrderService implementation that:
// - Takes OrderDbContext in constructor
// - Validates request (returns ValidationError if CustomerName is empty or Items is empty)
// - Creates Order, calculates Total from Items (sum of Quantity * UnitPrice), sets CreatedAt
// - Saves to DbContext and returns the Order
public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;

    public OrderService(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<OneOf<Order, ValidationError>> CreateOrderAsync(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName) || request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "Customer name is required and Items cannot be empty." };
        }

        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = request.Items.Sum(item => item.Quantity * item.UnitPrice),
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return new Order { Id = order.Id, CustomerName = order.CustomerName, Items = order.Items, Total = order.Total, CreatedAt = order.CreatedAt };
    }
}

// OrdersController : ControllerBase with:
// - POST /api/orders endpoint accepting CreateOrderRequest
// - Calls IOrderService.CreateOrder
// - Uses OneOf .Match to return Created (201) on success, BadRequest on ValidationError
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
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrderAsync(request);

        return result.Match<IActionResult>(
            order => CreatedAtRoute("Orders", new { id = order.Id }, order),
            validationError => BadRequest(validationError.Message)
        );
    }
}

// xUnit v3 tests:
// - Test OrderService happy path with EF Core InMemory database
// - Test OrderService validation error (empty customer name)
// - Test OrdersController with NSubstitute mock of IOrderService
public class OrderServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly IOrderService _orderService;
    private readonly DatabaseFixture _databaseFixture;

    public OrderServiceTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        _orderService = new OrderService(_databaseFixture.Context);
    }

    [Fact]
    public async Task CreateOrder_HappyPath()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = "John Doe",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Product A", Quantity = 1, UnitPrice = 10m },
                new OrderItem { ProductName = "Product B", Quantity = 2, UnitPrice = 5m }
            }
        };

        var result = await _orderService.CreateOrderAsync(request);

        Assert.IsType<Order>(result);
        var order = (Order)result;
        Assert.Equal("John Doe", order.CustomerName);
        Assert.Equal(15m, order.Total);
    }

    [Fact]
    public async Task CreateOrder_ValidationFailure()
    {
        var request = new CreateOrderRequest
        {
            CustomerName = "",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Product A", Quantity = 1, UnitPrice = 10m }
            }
        };

        var result = await _orderService.CreateOrderAsync(request);

        Assert.IsType<ValidationError>(result);
        var validationError = (ValidationError)result;
        Assert.Equal("Customer name is required and Items cannot be empty.", validationError.Message);
    }

    [Fact]
    public async Task OrdersController_HappyPath()
    {
        var mockOrderService = Substitute.For<IOrderService>();
        mockOrderService.CreateOrderAsync(Arg.Any<CreateOrderRequest>()).Returns(Task.FromResult(new Order { Id = 1, CustomerName = "John Doe", Items = new List<OrderItem>(), Total = 15m, CreatedAt = DateTime.UtcNow }));

        var controller = new OrdersController(mockOrderService);

        var result = await controller.CreateOrder(new CreateOrderRequest
        {
            CustomerName = "John Doe",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Product A", Quantity = 1, UnitPrice = 10m },
                new OrderItem { ProductName = "Product B", Quantity = 2, UnitPrice = 5m }
            }
        });

        Assert.Equal("201", result.StatusCode.ToString());
    }
}

// Assembly-scoped fixture for xUnit tests
[assembly: AssemblyFixture(typeof(DatabaseFixture))]

public class DatabaseFixture : IDisposable
{
    private readonly OrderDbContext _context;

    public DatabaseFixture()
    {
        var options = new DbContextOptions<OrderDbContext>
        {
            DatabaseName = "TestDb",
            UseInMemoryDatabase = true
        };

        _context = new OrderDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    public OrderDbContext Context => _context;

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }
}