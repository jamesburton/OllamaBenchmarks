using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagementVerticalSlice
{
    public class OrderItem : IEquatable<OrderItem>
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public bool Equals(OrderItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ProductName == other.ProductName &&
                   Quantity == other.Quantity &&
                   UnitPrice == other.UnitPrice;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OrderItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ProductName.GetHashCode();
                hashCode = (hashCode * 397) ^ Quantity.GetHashCode();
                hashCode = (hashCode * 397) ^ UnitPrice.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(OrderItem left, OrderItem right)
        {
            if ((object)left == null || (object)right == null) return false;
            return Equals(left, right);
        }

        public static bool operator !=(OrderItem left, OrderItem right)
        {
            return !(left == right);
        }
    }

    [GenerateOneOf]
    public partial class Order : IEquatable<Order>
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }

        [GenerateOneOf]
        public partial class OrderItem : IEquatable<OrderItem>
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }

            public bool Equals(OrderItem other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return ProductName == other.ProductName &&
                       Quantity == other.Quantity &&
                       UnitPrice == other.UnitPrice;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((OrderItem) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = ProductName.GetHashCode();
                    hashCode = (hashCode * 397) ^ Quantity.GetHashCode();
                    hashCode = (hashCode * 397) ^ UnitPrice.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(OrderItem left, OrderItem right)
            {
                if ((object)left == null || (object)right == null) return false;
                return Equals(left, right);
            }

            public static bool operator !=(OrderItem left, OrderItem right)
            {
                return !(left == right);
            }
        }

        [GenerateOneOf]
        public partial class ValidationError : IEquatable<ValidationError>
        {
            public string Message { get; set; }

            public bool Equals(ValidationError other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Message == other.Message;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ValidationError) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Message.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(ValidationError left, ValidationError right)
            {
                if ((object)left == null || (object)right == null) return false;
                return Equals(left, right);
            }

            public static bool operator !=(ValidationError left, ValidationError right)
            {
                return !(left == right);
            }
        }

        [GenerateOneOf]
        public partial class CreateOrderRequest
        {
            [Required]
            public string CustomerName { get; set; }
            [Range(1, 50)]
            public int ItemsCount { get; set; }
            [Range(1, 200)]
            public decimal TotalPrice { get; set; }

            public List<OrderItem> CreateOrderItems()
            {
                return Enumerable.Range(1, ItemsCount)
                    .Select(i => new OrderItem
                    {
                        ProductName = $"Product{i}",
                        Quantity = i,
                        UnitPrice = TotalPrice / (decimal)i
                    })
                    .ToList();
            }
        }

        public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
        {
            var items = request.CreateOrderItems();

            // Validate customer name and order items

            if (string.IsNullOrEmpty(request.CustomerName))
                throw new ValidationError("Customer name is required");

            if (!items.Any())
                throw new ValidationError("At least one item is required");

            // Calculate total price
            decimal totalPrice = 0;
            foreach (var item in items)
            {
                totalPrice += item.Quantity * item.UnitPrice;
            }

            var order = new Order
            {
                CustomerName = request.CustomerName,
                Items = items,
                Total = totalPrice,
                CreatedAt = DateTime.UtcNow
            };

            // Save to database

            return await _orderDbContext.CreateOrderAsync(order);
        }
    }

    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderRequest request);

        Task DeleteAsync(int orderId);
    }

    [GenerateOneOf]
    public partial class OrderDbContext : DbContext, IOrderService
    {
        public DbSet<Order> Orders { get; set; }
        private readonly string _connectionString;

        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
            _connectionString = options?.ConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await Orders.AddAsync(order);

            return order;
        }

        public async Task DeleteAsync(int orderId)
        {
            var result = await Orders.FindAsync(orderId);
            if (result == null) throw new ArgumentException("Order not found");

            await Orders.RemoveAsync(result);
        }
    }

    [GenerateOneOf]
    public partial class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("api/orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            return result switch
            {
                Order o => Ok(o),
                ValidationError v => BadRequest(v.Message),
                _ => throw new ArgumentException("Unexpected error"),
            };
        }
    }

    [GenerateOneOf]
    public partial class OrdersTests : IAsyncLifetime
    {
        private readonly IOrderService _orderService;
        private readonly TestContext _testContext;

        public OrdersTests(IOrderService orderService, TestContext testContext)
        {
            _orderService = orderService;
            _testContext = testContext;
        }

        [Fact]
        public async Task CreateOrder_HappyPath()
        {
            var fixture = await _testContext.GetFixture<DatabaseFixture>();
            var request = new CreateOrderRequest { CustomerName = "John Doe", ItemsCount = 10, TotalPrice = 50 };
            var result = await _orderService.CreateOrderAsync(request);
            Assert.IsType<Order>(result);

            // Save to database

            var order = fixture.OrderDbContext.Orders.FirstOrDefault(o => o.Id == result.Id);
            Assert.NotNull(order);
            Assert.Equal("John Doe", order.CustomerName);
            Assert.Equal(10, order.Items.Count);
            Assert.Equal(5.0m, order.TotalPrice);
        }

        [Fact]
        public async Task CreateOrder_ValidationErrors()
        {
            var fixture = await _testContext.GetFixture<DatabaseFixture>();
            var request = new CreateOrderRequest { CustomerName = null, ItemsCount = 1 };
            var result = await _orderService.CreateOrderAsync(request);
            Assert.IsType<ValidationError>(result);

            // Save to database

            var order = fixture.OrderDbContext.Orders.FirstOrDefault(o => o.Id == default(int));
            Assert.Null(order);
        }

        public async Task InitializeAsync()
        {
            var fixture = new DatabaseFixture();
            await fixture.OrderDbContext.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            // Clean up
        }
    }

    [GenerateOneOf]
    public partial class DatabaseFixture : IAsyncLifetime
    {
        private readonly DbContextOptions _dbContextOptions;

        public DatabaseFixture(DbContextOptions dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        [Fact]
        public async Task CreateDatabase()
        {
            var fixture = await new TestContext().GetFixture<DatabaseFixture>();
            await fixture.OrderDbContext.Database.MigrateAsync();
        }

        public async Task InitializeAsync()
        {
            // Clean up
        }

        public async Task DisposeAsync()
        {
            // Clean up
        }
    }
}