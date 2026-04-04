using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;

// 1. OrderItem class
public class OrderItem
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// 2. Order class
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 3. CreateOrderRequest class
public class CreateOrderRequest
{
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; }
}

// 4. ValidationError class
public class ValidationError
{
    public string Message { get; set; }
}

// 5. IOrderRepository interface
public interface IOrderRepository
{
    void Save(Order order);
    Order? GetById(int id);
}

// 6. OrderService class
public class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
    {
        // Validation 1: Customer Name
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return new ValidationError { Message = "Customer name is required" };
        }

        // Validation 2: Items
        if (request.Items == null || !request.Items.Any())
        {
            return new ValidationError { Message = "At least one item is required" };
        }

        // Calculation
        decimal total = request.Items.Sum(item => item.Quantity * item.UnitPrice);

        // Creation
        var order = new Order
        {
            // Assuming ID generation happens here or in the repository, 
            // but for simplicity in this slice, we'll use a placeholder ID.
            Id = 0, 
            CustomerName = request.CustomerName,
            Items = request.Items,
            Total = total,
            CreatedAt = DateTime.UtcNow
        };

        // Persistence
        _repository.Save(order);

        // Success return
        return order;
    }
}