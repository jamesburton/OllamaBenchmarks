using OneOf;

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
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class ValidationError
{
    public string Message { get; set; } = string.Empty;
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetById(int id);
}

public class OrderService
{
    private readonly IOrderRepository _repository;
    private int _nextId = 1;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        return request switch
        {
            {
                CustomerName: null or "",
                Items: null or {}
            } => new ValidationError { Message = "Customer name is required" },

            {
                Items: null or {}
            } => new ValidationError { Message = "At least one item is required" },

            {
                CustomerName: not null or not empty,
                Items: not null and not {} items
            } => CreateOrderImpl(request)
        };

        [System.Runtime.CompilerServices.CompilerServices.Unsafe] 
        static OneOf<Order, ValidationError> CreateOrderImpl(CreateOrderRequest request)
        {
            var order = new Order
            {
                Id = _nextId++,
                CustomerName = request.CustomerName,
                Items = request.Items.Select(i => new OrderItem
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList(),
                CreatedAt = DateTime.UtcNow
            };

            order.Total = order.Items.Sum(item => item.Quantity * item.UnitPrice);
            order.Items.ForEach(item => _repository.Save(order));
            return order;
        }
    }
}