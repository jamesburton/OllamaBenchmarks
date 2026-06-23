public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
{
    if (string.IsNullOrEmpty(request.CustomerName))
        return new ValidationError { Message = "Customer name is required" };

    if (request.Items == null || request.Items.Count == 0)
        return new ValidationError { Message = "At least one item is required" };

    Order order = new Order
    {
        Id = 0,
        CustomerName = request.CustomerName,
        Items = request.Items
    };

    decimal total = 0m;
    foreach (var item in request.Items)
    {
        total += item.Quantity * item.UnitPrice;
    }
    order.Total = total;
    order.CreatedAt = DateTime.UtcNow;

    repository.Save(order);
    return order;
}

using OneOf;

public class OrderItem { ... }
public class Order { ... }
...

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
    public string Message { get; set; }
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

public class OrderService
{
    private final IOrderRepository repository;

    public OrderService(IOrderRepository repository)
    {
        repository = repository;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        Order order = new Order
        {
            Id = 0,
            CustomerName = request.CustomerName,
            Items = request.Items
        };

        decimal total = 0m;
        foreach (var item in request.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        order.Total = total;
        order.CreatedAt = DateTime.UtcNow;
        repository.Save(order);
        return order;
    }
}

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
    public string Message { get; set; }
}

public interface IOrder Repository
{
    void Save(Order order);
    Order? GetId(int id);
}

public class OrderService
{
    private final IOrderRepository repository;

    public OrderService(IOrder Repository)
    {
        repository = repository;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        Order order = new Order
        {
            Id = 0,
            CustomerName = request.CustomerName,
            Items = request.Items
        };

        decimal total = 0m;
        foreach (var item in request.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        order.Total = total;
        order.CreatedAt = DateTime.UtcNow;
        repository.Save(order);
        return order;
    }
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

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
    public string Message { get; set; }
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

public class OrderService
{
    private final IOrder Repository;

    public OrderService(IOrder Repository)
    {
        Repository = Repository;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        Order order = new Order
        {
            Id = 0,
            CustomerName = request.CustomerName,
            Items = request.Items
        };

        decimal total = 0m;
        foreach (var item in request.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        order.Total = total;
        order.CreatedAt = DateTime.UtcNow;
        repository.Save(order);
        return order;
    }
}

private final IOrder Repository;

public OrderService(IOrder Repository)
{
    Repository = Repository;
}

private final IOrder Repository;

public OrderService(IOrder Repository)
{
    Repository = repository;
}

public OrderService(IOrderRepository repository)
{
    this.repository = repository;
}

private final IOrderRepository repository;

public OrderService(IOrder Repository)
{
    repository = repository;
}

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
    public string Message { get; set; }
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

public class OrderService
{
    private final IOrder Repository repository;

    public OrderService(IOrder Repository)
    {
        repository = repository;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        Order order = new Order
        {
            Id = 0,
            CustomerName = request.CustomerName,
            Items = request.Items
        };

        decimal total = 0m;
        foreach (var item in request.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        order.Total = total;
        order.CreatedAt = DateTime.UtcNow;
        repository.Save(order);
        return order;
    }
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

public OrderService(IOrderRepository repo)
{
    repository = repo;
}

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
    public string Message { get; set; }
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

public class OrderService
{
    private final IOrder Repository repository;

    public Order Service(IOrderRepository repo)
    {
        repository = repo;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        Order order = new Order
        {
            Id = 0,
            CustomerName = request.CustomerName,
            Items = request.Items
        };

        decimal total = 0m;
        foreach (var item in request.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        order.Total = total;
        order.CreatedAt = DateTime.UtcNow;
        repository.Save(order);
        return order;
    }
}

public OrderService(IOrderRepository repo)
{
    repository = repo;
}

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
    public string Message { get; set; }
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

public class OrderService
{
    private final IOrder Repository repository;

    public Order Service(IOrderRepository repo)
    {
        repository = repo;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        Order order = new Order
        {
            Id = 0,
            CustomerName = request.CustomerName,
            Items = request.Items
        };

        decimal total = 0m;
        foreach (var item in request.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        order.Total = total;
        order.CreatedAt = DateTime.UtcNow;
        repository.Save(order);
        return order;
    }
}

private final IOrderRepository repository;

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
    public string Message { get; set; }
}

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetId(int id);
}

public class OrderService
{
    private final IOrder Repository repository;

    public Order Service(IOrderRepository repo)
    {
        repository = repo;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        if (string.IsNullOrEmpty(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        Order order = new Order
        {
            Id = 0,
            CustomerName = request.CustomerName,
            Items = request.Items
        };

        decimal total = 0m;
        foreach (var item in request.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        order.Total = total;
        order.CreatedAt = DateTime.UtcNow;
        repository.Save(order);
        return order;
    }
}