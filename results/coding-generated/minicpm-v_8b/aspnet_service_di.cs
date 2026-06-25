using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService
{
    public interface IOrderRepository
    {
        Task AddOrder(string warehouseCode, int maxRetries);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly IEmailNotifier _emailNotifier;

        public OrderRepository(IEmailNotifier emailNotifier)
        {
            _emailNotifier = emailNotifier;
        }

        public async Task AddOrder(string warehouseCode, int maxRetries)
        {
            // Implementation goes here...
        }
    }

    public interface IOrderService
    {
        Task PlaceOrder();
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly OrderSettings _settings;

        public OrderService(IOrderRepository orderRepository, OrderSettings settings)
        {
            _orderRepository = orderRepository;
            _settings = settings;
        }

        public async Task PlaceOrder()
        {
            // Implementation goes here...
        }
    }

    public interface IEmailNotifier
    {
        void Notify(string email);
    }

    public class EmailNotifier : IEmailNotifier
    {
        private readonly string _email;

        public EmailNotifier(string email)
        {
            _email = email;
        }

        public void Notify(string email)
        {
            // Implementation goes here...
        }
    }

    public class OrderSettings
    {
        public string WarehouseCode { get; set; }
        public int MaxRetries { get; set; }
    }

    static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrderServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IEmailNotifier, EmailNotifier>();
            services.AddTransient<OrderSettings>(c => config.GetSection("Orders").Get<OrderSettings>());
            return services;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddOrderServices(config =>
                config.WarehouseCode = "ABC123", 
                config.MaxRetries = 5);

            var app = builder.Build();
            app.Run();
        }
    }
}