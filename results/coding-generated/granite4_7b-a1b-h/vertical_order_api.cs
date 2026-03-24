using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace OrderManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController
    {
        private readonly IOrderService _orderService;
        private readonly DbContextOptions<OrderDbContext> _options;

        public OrdersController(IOrderService orderService, DbContextOptions<OrderDbContext> options)
        {
            _orderService = orderService;
            _options = options;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var result = await _orderService.CreateOrder(request);
            return result.Match<IActionResult>(
                order => Created($"api/orders/{order.Id}", order),
                err => BadRequest(err.Message)
            );
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public record CreateOrderRequest(string CustomerName, List<OrderItem> Items);

    public record ValidationError(string Message);

    public interface IOrderService
    {
        Task<OneOf<Order, ValidationError>> CreateOrder(CreateOrderRequest request);
    }

    public class OrderDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options) { }
    }

    public class OrderService : IOrderService
    {
        private readonly OrderDbContext _context;

        public OrderService(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<OneOf<Order, ValidationError>> CreateOrder(CreateOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                return OneOf<ValidationError>.FromT1(new ValidationError("CustomerName is required"));
            }

            if (!request.Items.Any())
            {
                return OneOf<ValidationError>.FromT1(new ValidationError("At least one item is required"));
            }

            var order = new Order
            {
                CustomerName = request.CustomerName,
                Items = request.Items,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in request.Items)
            {
                order.Total += item.Quantity * item.UnitPrice;
            }

            await _context.AddAsync(order);
            await _context.SaveChangesAsync();

            return OneOf<Order>.FromT0(order);
        }
    }

    public class OrderTests
    {
        private readonly ITestOutputHelper _output;

        public OrderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task CreateOrder_HappyPath()
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase("OrderTests")
                .Options;

            using var context = new OrderDbContext(options);
            var orderService = new OrderService(context);

            var request = new CreateOrderRequest("John Doe", new List<OrderItem>
            {
                new("Product A", 2, 10.0m),
                new("Product B", 1, 20.0m)
            });

            var result = await orderService.CreateOrder(request);

            result.Match(
                order => { order.Total.Should().Be(40.0m); },
                err => { throw new Exception("Order should not be a validation error"); }
            );
        }

        [Fact]
        public async Task CreateOrder_CustomerNameMissing()
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase("OrderTests")
                .Options;

            using var context = new OrderDbContext(options);
            var orderService = new OrderService(context);

            var request = new CreateOrderRequest(string.Empty, new List<OrderItem>
            {
                new("Product A", 2, 10.0m)
            });

            var result = await orderService.CreateOrder(request);

            result.Match(
                order => { throw new Exception("Order should not be created"); },
                err => { err.Message.Should().Be("CustomerName is required"); }
            );
        }

        [Fact]
        public async Task CreateOrder_NoItems()
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase("OrderTests")
                .Options;

            using var context = new OrderDbContext(options);
            var orderService = new OrderService(context);

            var request = new CreateOrderRequest("John Doe", new List<OrderItem>());

            var result = await orderService.CreateOrder(request);

            result.Match(
                order => { throw new Exception("Order should not be created"); },
                err => { err.Message.Should().Be("At least one item is required"); }
            );
        }
    }
}