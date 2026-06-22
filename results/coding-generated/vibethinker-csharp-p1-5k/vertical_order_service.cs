using OneOf;

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetById(int id);
}

public class OrderItem
{
            string ProductName;
            int Quantity;
            decimal UnitPrice;
        }

public class Order
{
            int Id;
            string CustomerName;
            List<OrderItem> Items;
            decimal Total;
            DateTime CreatedAt;
        }

public class CreateOrderRequest
{
            string CustomerName;
            List<OrderItem> Items;
        }

public class ValidationError
{
            string Message;
        }

public class OrderService
{
            private readonly IOrderRepository repository;

            public OrderService(IOrderRepository repository)
            {
                this.repository = repository;
            }

            public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
            {
                if (string.IsNullOrEmpty(request.CustomerName))
                    return new ValidationError { Message = "Customer name is required" };

                if (request.Items == null || request.Items.Count() < 1)
                    return new ValidationError { Message = "At least one item is required" };

                var order = new Order
                {
                    CustomerName = request.CustomerName,
                    Items = request.Items
                };

                order.Id = Guid.NewGuid().ToString();
                order.Total = order.Items.Sum(item => item.Quantity * item.UnitPrice);
                order.CreatedAt = DateTime.UtcNow;

                repository.Save(order);

                return order;
            }
        }