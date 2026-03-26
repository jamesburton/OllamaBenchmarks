using OneOf;

public class OrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) => _repo = repo;

    public void CreateOrder(CreateOrderRequest request)
    {
        var customerName = request.CustomerName;
        var items = request.Items;

        if (customerName == null || string.IsNullOrEmpty(customerName))
        {
            var error = new ValidationError("Customer name required");
            _repo.Save(new Order { CustomerName = customerName, Items = items });
            return error;
        }

        if (items == null || items.Count == 0)
        {
            var error = new ValidationError("At least one item required");
            _repo.Save(new Order { CustomerName = customerName, Items = items });
            return error;
        }

        var order = new Order { Id = DateTime.UtcNow, CustomerName = customerName, Items = items, Total = items.Sum(item => item.Quantity * item.UnitPrice), CreatedAt = DateTime.UtcNow };
        _repo.Save(order);
        return new Order { Id = order.Id, CustomerName = customerName, Items = items, Total = order.Total, CreatedAt = order.CreatedAt };
    }
}