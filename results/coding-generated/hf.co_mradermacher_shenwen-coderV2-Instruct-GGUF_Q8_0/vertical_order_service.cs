using System;
using OneOf;

namespace OrderService
{
    public interface IOrderRepository
    {
        void Save(Order order);
        Order? GetById(int id);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
        {
            if (request.CustomerName == null || string.IsNullOrEmpty(request.CustomerName))
                return new ValidationError("Customer name is required");

            if (request.Items == null || request.Items.Count == 0)
                return new ValidationError("At least one item is required");

            var order = new Order
            {
                CustomerName = request.CustomerName,
                Items = request.Items.Select(item => new OrderItem { ProductName = item.ProductName, Quantity = item.Quantity, UnitPrice = item.UnitPrice })
                    .ToList(),
                Total = items.Sum(item => item.UnitPrice * item.Quantity),
                CreatedAt = DateTime.UtcNow
            };

            return _orderRepository.Save(order);
        }
    }

    public class CreateOrderRequest
    {
        public string CustomerName { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}