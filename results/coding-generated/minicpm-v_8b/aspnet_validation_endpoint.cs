using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrderService
{
    public record CreateOrderRequest(string Name, decimal Price, int Quantity);

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiredAttribute : ValidationAttribute
    {
        protected override string? IsValid(object value)
        {
            if (value is null) return "Name cannot be empty";
            return base.IsValid(value);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RangeAttribute : ValidationAttribute
    {
        private readonly int _min;
        private readonly int _max;

        public RangeAttribute(int min, int max)
        {
            if (min > max) throw new ArgumentException("Min must be less than or equal to Max");
            _min = min;
            _max = max;
        }

        protected override string? IsValid(object value)
        {
            if (_min <= 0 && _max >= 100) return "Quantity should not exceed 999";
            var num = (int)value;
            if (num < _min || num > _max) return $"Quantity must be between {_min} and {_max}";
            return base.IsValid(value);
        }
    }

    public class OrderService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Order> CreateOrder(CreateOrderRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            return await client.PostAsJsonAsync("/api/orders", request);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class OrderEndpointsAttribute : Attribute
    {
        private readonly string _path;

        public OrderEndpointsAttribute(string path)
        {
            _path = path;
        }

        public void MapOrderEndpoints(WebApplication app)
        {
            var orderService = new OrderService(_httpClientFactory);
            app.MapPost(_path, orderService.CreateOrder);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ProblemDetailsAttribute : ValidationAttribute
    {
        protected override string? IsValid(object value)
        {
            if (value is null) return "Unexpected error occurred";
            return base.IsValid(value);
        }
    }

    public static void Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args).UseStartup<Startup>().Build();
        OrderEndpointsAttribute orderEndpointsAttribute = new("api/orders");
        orderEndpointsAttribute.MapOrderEndpoints(app);

        app.Run();
    }
}