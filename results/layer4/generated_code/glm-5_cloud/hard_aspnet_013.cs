using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VerticalSlice.ScopedServiceDemo
{
    // --- Domain Entities ---
    public record Product(int Id, string Name, decimal Price);
    public record Order(int Id, int ProductId, int Qty, decimal Total);

    // --- Repository Interfaces ---
    public interface IProductRepository
    {
        Task<Product?> FindAsync(int id, CancellationToken ct);
        Task AddAsync(Product product, CancellationToken ct);
    }

    public interface IOrderRepository
    {
        Task<Order?> FindAsync(int id, CancellationToken ct);
        Task AddAsync(Order order, CancellationToken ct);
    }

    // --- Unit of Work Interface ---
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        Task<int> CommitAsync(CancellationToken ct);
    }

    // --- Infrastructure Implementation ---
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<int, Product> _products = new();
        private readonly Dictionary<int, Order> _orders = new();

        // Simulating auto-increment logic
        private int _lastProductId = 0;
        private int _lastOrderId = 0;

        public IProductRepository Products { get; }
        public IOrderRepository Orders { get; }

        public InMemoryUnitOfWork()
        {
            // In a real scenario, repositories might be injected or created here
            // For this demo, we pass the unit of work's internal storage to the repositories
            Products = new InMemoryProductRepository(this);
            Orders = new InMemoryOrderRepository(this);
        }

        // Simulated Scoped Lifetime behavior: 
        // In a real DI container, this would be handled by the container.
        // Here we just implement IDisposable to satisfy the interface.
        public void Dispose()
        {
            // Cleanup resources if necessary
            GC.SuppressFinalize(this);
        }

        public Task<int> CommitAsync(CancellationToken ct)
        {
            // In a real DB context, this would call SaveChangesAsync
            // Here, data is already added to dictionaries in-memory, so we just return success.
            return Task.FromResult(1); 
        }

        // Internal repository implementations acting on the Unit of Work's storage
        private class InMemoryProductRepository : IProductRepository
        {
            private readonly InMemoryUnitOfWork _uow;
            public InMemoryProductRepository(InMemoryUnitOfWork uow) => _uow = uow;

            public Task<Product?> FindAsync(int id, CancellationToken ct)
            {
                _uow._products.TryGetValue(id, out var product);
                return Task.FromResult(product);
            }

            public Task AddAsync(Product product, CancellationToken ct)
            {
                var newId = product.Id == 0 ? ++_uow._lastProductId : product.Id;
                var persistedProduct = product with { Id = newId };
                _uow._products[newId] = persistedProduct;
                return Task.CompletedTask;
            }
        }

        private class InMemoryOrderRepository : IOrderRepository
        {
            private readonly InMemoryUnitOfWork _uow;
            public InMemoryOrderRepository(InMemoryUnitOfWork uow) => _uow = uow;

            public Task<Order?> FindAsync(int id, CancellationToken ct)
            {
                _uow._orders.TryGetValue(id, out var order);
                return Task.FromResult(order);
            }

            public Task AddAsync(Order order, CancellationToken ct)
            {
                var newId = order.Id == 0 ? ++_uow._lastOrderId : order.Id;
                var persistedOrder = order with { Id = newId };
                _uow._orders[newId] = persistedOrder;
                return Task.CompletedTask;
            }
        }
    }

    // --- Application Service ---
    public class OrderCreationService
    {
        private readonly IUnitOfWork _uow;

        public OrderCreationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Order> CreateOrderAsync(int productId, int qty, CancellationToken ct)
        {
            // 1. Find Product
            var product = await _uow.Products.FindAsync(productId, ct);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} was not found.");
            }

            // 2. Calculate Total
            var total = product.Price * qty;

            // 3. Create Order (ID 0 indicates new entity to be generated)
            var order = new Order(Id: 0, ProductId: productId, Qty: qty, Total: total);

            // 4. Add to Repository
            await _uow.Orders.AddAsync(order, ct);

            // 5. Commit Transaction
            await _uow.CommitAsync(ct);

            // Note: In this in-memory implementation, the 'order' object variable 
            // does not automatically update with the new ID from the dictionary 
            // unless we fetch it again or the AddAsync method is modified to return it.
            // To strictly follow the interface, we return the object as is, 
            // or we assume the AddAsync persisted it.
            // For demonstration, we return the created object concept.
            return order; 
        }
    }

    // --- Program Entry Point (Demonstration) ---
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Simulating a scope creation
            using (var uow = new InMemoryUnitOfWork())
            {
                var productService = new OrderCreationService(uow);

                // Seed a product first directly via repository
                await uow.Products.AddAsync(new Product(1, "Laptop", 1000.00m), CancellationToken.None);
                await uow.CommitAsync(CancellationToken.None);

                Console.WriteLine("Product 'Laptop' seeded.");

                // Create an order
                try 
                {
                    var order = await productService.CreateOrderAsync(productId: 1, qty: 2, CancellationToken.None);
                    Console.WriteLine($"Order Created: ProductId={order.ProductId}, Qty={order.Qty}, Total={order.Total}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}