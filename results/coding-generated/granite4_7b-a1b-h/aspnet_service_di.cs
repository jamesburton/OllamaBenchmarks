using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace MyApp
{
    public interface IOrderRepository
    {
        // Marker interface
    }

    public class OrderRepository : IOrderRepository
    {
        // Implementation
    }

    public interface IOrderService
    {
        // Marker interface
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
        // Implementation
    }

    public interface IEmailNotifier
    {
        // Marker interface
    }

    public class EmailNotifier : IEmailNotifier
    {
        // Implementation
    }

    public class OrderSettings
    {
        public string WarehouseCode { get; set; }
        public int MaxRetries { get; set; }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<OrderSettings>(config.GetSection("Orders"));
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddTransient<IEmailNotifier, EmailNotifier>();
            services.BindServices(config, typeof(OrderSettings));
            return services;
        }
    }
}