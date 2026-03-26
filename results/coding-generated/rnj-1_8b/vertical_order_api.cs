using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using NSubstitute;
using OneOf;
using Xunit;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = null!;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

[Owned]
public class OrderItem
{
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = null!;
}

public class ValidationError
{
    public string Message { get; set; } = null!;
}

public interface IOrderService
{
    OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request);
}

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
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || !request.Items.Any())
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        var order = new Order
        {
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = request.Items.Sum(i => i.Quantity * i.UnitPrice),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();

        return order;
    }
}

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; } = null!;

    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
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
    public IActionResult CreateOrder(CreateOrderRequest request)
    {
        var result = _orderService.CreateOrder(request);

        return result.Match<Order, ValidationError>(
            order => Created(order.Id, order),
            error => BadRequest(error.Message));
    }
}

public class OrdersControllerTests
{
    [Fact]
    public void CreateOrder_Success()
    {
        var dbContext = new OrderDbContext(new DbContextOptionsBuilder<OrderDbContext>().UseInMemoryDatabase("test").Options);
        var service = new OrderService(dbContext);
        var request = new CreateOrderRequest
        {
            CustomerName = "Alice",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10m }
            }
        };

        var result = service.CreateOrder(request);

        result.Match<Order, ValidationError>(
            order =>
            {
                order.Should().NotBeNull();
                order.Total.Should().Be(20m);
            },
            error => throw new Exception(error.Message));
    }

    [Fact]
    public void CreateOrder_ValidationError()
    {
        var dbContext = new OrderDbContext(new DbContextOptionsBuilder<OrderDbContext>().UseInMemoryDatabase("test").Options);
        var service = new OrderService(dbContext);
        var request = new CreateOrderRequest
        {
            CustomerName = "",
            Items = new List<OrderItem>()
        };

        var result = service.CreateOrder(request);

        result.Match<Order, ValidationError>(
            order => throw new Exception("Expected validation error"),
            error => error.Message.Should().Be("Customer name is required"));
    }

    [Fact]
    public void OrdersController_Success()
    {
        var dbContext = new OrderDbContext(new DbContextOptionsBuilder<OrderDbContext>().UseInMemoryDatabase("test").Options);
        var service = SubstituteFor<IOrderService>();
        service.CreateOrder(Arg.Any<CreateOrderRequest>()).Returns(new OneOf<Order, ValidationError>(
            new Order
            {
                Id = 1,
                CustomerName = "Alice",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10m }
                },
                Total = 20m,
                CreatedAt = DateTime.UtcNow
            }));

        var controller = new OrdersController(service);
        var result = controller.CreateOrder(new CreateOrderRequest
        {
            CustomerName = "Alice",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10m }
            }
        });

        result.Should().BeOfType<CreatedResult>();
        var created = (CreatedResult)result;
        created.Value.Should().BeOfType<Order>();
    }
}