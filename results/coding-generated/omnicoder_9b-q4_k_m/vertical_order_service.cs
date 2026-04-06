using System;
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
            public List<OrderItem> Items { get; set; }
            public decimal Total { get; set; }
            public DateTime CreatedAt { get; set; }
        }

public class CreateOrderRequest
        {
            public string CustomerName { get; set; }
            public List<OrderItem> Items { get; set; }
        }

public class ValidationError
        {
            public string Message { get; set; }
        }

public interface IOrderRepository
        {
            void Save(Order order);
            Order? GetById(int id);
        }

public class OrderService
        {
            private readonly IOrderRepository _repository;

            public OrderService(IOrderRepository repository)
            {
                _repository = repository;
            }

            public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
            {
                if (string.IsNullOrWhiteSpace(request.CustomerName))
                {
                    return OneOf.FromT1(new ValidationError { Message = "Customer name is required" });
                }

                if (request.Items == null || request.Items.Count == 0)
                {
                    return OneOf.FromT1(new ValidationError { Message = "At least one item is required" });
                }

                var order = new Order
                {
                    CustomerName = request.CustomerName,
                    Items = new List<OrderItem>(request.Items), // Copy items to avoid mutation issues if needed, though prompt implies direct usage. Let's copy to be safe or just assign. Prompt says "sets CreatedAt... calls repository.Save". Usually, we construct the Order object.
                    CreatedAt = DateTime.UtcNow
                };

                // Calculate Total
                decimal total = 0;
                foreach (var item in request.Items)
                {
                    total += item.Quantity * item.UnitPrice;
                }
                order.Total = total;

                _repository.Save(order);

                return OneOf.FromT0(order);
            }
        }

using System;
    using System.Collections.Generic;
    using OneOf;

using System;
    using System.Collections.Generic;
    using OneOf;

using OneOf;
    using System;
    using System.Collections.Generic;

using OneOf;
    using System;
    using System.Collections.Generic;

using OneOf;
    using System;
    using System.Collections.Generic;

using OneOf;
    using System;
    using System.Collections.Generic;

using OneOf;
    using System;
    using System.Collections.Generic;

using OneOf;
    using System;
    using System.Collections.Generic;

using OneOf;
    using System;
    using System.Collections.Generic;

using OneOf;
    using System;
    using System.Collections.Generic;

using OneOf;
    using System;
    using System.Collections.Generic;

using OneOf;
    using System;
    using System.Collections.Generic;