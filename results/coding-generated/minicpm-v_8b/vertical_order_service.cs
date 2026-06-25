using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using NSubstitute;

namespace OrderServiceVerticalSlice
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
                return ((ProductName.GetHashCode() * 397) ^ Quantity.GetHashCode()) ^
                       UnitPrice.GetHashCode();
            }
        }

        public static bool operator ==(OrderItem left, OrderItem right)
        {
            if (ReferenceEquals(null, left)) return ReferenceEquals(null, right);
            if (ReferenceEquals(left, right)) return true;
            return Equals(left, right);
        }

        public static bool operator !=(OrderItem left, OrderItem right) => !(left == right);

        public override string ToString()
        {
            return $"Product: {ProductName}, Quantity: {Quantity}, Unit Price: {UnitPrice}";
        }
    }

    [AwesomeAssertions]
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }

        [AwesomeAssertions]
        public Order()
        {
            CreatedAt = DateTime.UtcNow;
        }

        [AwesomeAssertions]
        public void Validate()
        {
            if (string.IsNullOrEmpty(CustomerName))
                throw new ArgumentException("Customer name is required");
            if (Items == null || Items.Count == 0)
                throw new ArgumentException("At least one item is required");

            Total = CalculateTotal();
        }

        private decimal CalculateTotal()
        {
            return Items.Sum(item => item.Quantity * item.UnitPrice);
        }
    }

    [AwesomeAssertions]
    public class CreateOrderRequest
    {
        public string CustomerName { get; set; }
        public List<OrderItem> Items { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(CustomerName))
                throw new ArgumentException("Customer name is required");
            if (Items == null || Items.Count == 0)
                throw new ArgumentException("At least one item is required");
        }
    }

    [AwesomeAssertions]
    public class ValidationError : IEquatable<ValidationError>
    {
        public string Message { get; set; }

        public bool Equals(ValidationError other) => ReferenceEquals(this, other) || Message == other.Message;

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
                return Message.GetHashCode();
            }
        }

        public static bool operator ==(ValidationError left, ValidationError right)
        {
            if (ReferenceEquals(null, left)) return ReferenceEquals(null, right);
            if (ReferenceEquals(left, right)) return true;
            return Equals(left, right);
        }

        public static bool operator !=(ValidationError left, ValidationError right) => !(left == right);

        public override string ToString()
        {
            return $"Error: {Message}";
        }
    }

    [AwesomeAssertions]
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [AwesomeAssertions]
        public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
        {
            if (string.IsNullOrEmpty(request.CustomerName))
                return new ValidationError("Customer name is required");
            if (request.Items == null || request.Items.Count == 0)
                return new ValidationError("At least one item is required");

            var order = new Order
            {
                CustomerName = request.CustomerName,
                Items = request.Items,
                Total = CalculateTotal(),
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                Validate(order);
                _orderRepository.Save(order);
                return OneOf<Order, ValidationError>.FromT0(order);
            }
            catch (Exception ex)
            {
                return new ValidationError(ex.Message);
            }
        }

        private decimal CalculateTotal()
        {
            return request.Items.Sum(item => item.Quantity * item.UnitPrice);
        }

        [AwesomeAssertions]
        public void Validate(Order order)
        {
            if (string.IsNullOrEmpty(order.CustomerName))
                throw new ArgumentException("Customer name is required");
            if (order.Items == null || order.Items.Count == 0)
                throw new ArgumentException("At least one item is required");

            order.Validate();
        }
    }

    [AwesomeAssertions]
    public class OrderRepository
    {
        private readonly List<Order> _orders = new List<Order>();

        public void Save(Order order)
        {
            _orders.Add(order);
        }

        public Order? GetById(int id) => _orders.FirstOrDefault(o => o.Id == id);

        public IEnumerable<Order> GetAll() => _orders;
    }
}