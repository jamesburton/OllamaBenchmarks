using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyNamespace
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrders();
    }

    public class Order
    {
        public string WarehouseCode { get; set; }
        public int MaxRetries { get; set; }
    }

    public interface IOrderService
    {
        Task<Order> CreateOrder(string name, int qty);
        Task<Order> UpdateOrder(int orderId, string name, int qty);
        Task DeleteOrder(int orderId);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly IEmailNotifier _emailNotifier;

        public OrderRepository(IEmailNotifier emailNotifier)
        {
            _emailNotifier = emailNotifier;
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            // Implement logic to retrieve orders from a database or other data source.
            return new List<Order>();
        }
    }

    public class EmailNotifier
    {
        private readonly string _warehouseCode;

        public EmailNotifier(string warehouseCode)
        {
            _warehouseCode = warehouseCode;
        }

        public void SendNotification(string name, int qty)
        {
            // Implement logic to send an email notification.
        }
    }

    public interface IEmailNotifier
    {
        void SendNotification(string name, int qty);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailNotifier _emailNotifier;

        public OrderService(IOrderRepository orderRepository, IEmailNotifier emailNotifier)
        {
            _orderRepository = orderRepository;
            _emailNotifier = emailNotifier;
        }

        public async Task<Order> CreateOrder(string name, int qty)
        {
            // Implement logic to create a new order.
            return new Order { WarehouseCode = "ABC", MaxRetries = 3 };
        }

        public async Task<Order> UpdateOrder(int orderId, string name, int qty)
        {
            var existingOrder = await _orderRepository.GetOrders().FirstOrDefaultAsync(o => o.Id == orderId);
            if (existingOrder == null) throw new ArgumentException("Order not found.");

            // Implement logic to update the order.
            return new Order { WarehouseCode = "ABC", MaxRetries = 3 };
        }

        public async Task DeleteOrder(int orderId)
        {
            var existingOrder = await _orderRepository.GetOrders().FirstOrDefaultAsync(o => o.Id == orderId);
            if (existingOrder == null) throw new ArgumentException("Order not found.");

            // Implement logic to delete the order.
        }
    }

    public class OrderSettings
    {
        [Config]
        public string WarehouseCode { get; set; } = "ABC";

        [Config]
        public int MaxRetries { get; set; } = 3;
    }

    public static class ServiceCollectionExtensions
    {
        public static void AddOrderServices(this IServiceCollection services, IConfiguration config)
        {
            // Register IOrderRepository → OrderRepository as scoped.
            services.AddScoped<IOrderRepository, OrderRepository>();

            // Register IEmailNotifier → EmailNotifier as transient.
            services.AddTransient<IEmailNotifier, EmailNotifier>();

            // Bind OrderSettings from config section "Orders" using the options pattern.
            var orderSettings = config.GetSection("Orders").Get<Dictionary<string, object>>();
            if (orderSettings == null) throw new ArgumentException("Order settings not found in configuration.");

            var warehouseCode = orderSettings["WarehouseCode"] as string;
            var maxRetries = orderSettings["MaxRetries"] as int;

            // Register IOrderService → OrderService using the scoped IOrderRepository.
            services.AddScoped<IOrderService, (IOrderRepository orderRepository) =>
                new OrderService(orderRepository, warehouseCode));

            // Register EmailNotifier with WarehouseCode and MaxRetries from config section "Orders".
            var emailNotifier = new EmailNotifier(warehouseCode);
            if (maxRetries > 0)
            {
                emailNotifier.MaxRetries = maxRetries;
            }

            services.AddSingleton<IEmailNotifier, emailNotifier>();
        }
    }
}