*   Input: API reference for ASP.NET Core 10.
    *   Goal: Generate a single C# file.
    *   Requirements:
        1.  `IOrderRepository` (interface).
        2.  `OrderRepository` (class, implements `IOrderRepository`, parameterless constructor).
        3.  `IOrderService` (interface).
        4.  `OrderService` (class, implements `IOrderService`, parameterless constructor).
        5.  `IEmailNotifier` (interface).
        6.  `EmailNotifier` (class, implements `IEmailNotifier`, parameterless constructor).
        7.  `OrderSettings` class (properties: `WarehouseCode` string, `MaxRetries` int).
        8.  `ServiceCollectionExtensions` static class with `AddOrderServices` method.
        9.  `AddOrderServices` logic:
            *   `IOrderRepository` $\to$ `OrderRepository` (Scoped).
            *   1`IOrderService` $\to$ `OrderService` (Scoped).
            *   `IEmailNotifier` $\to$ `EmailNotifier` (Transient).
            *   Bind `OrderSettings` from config section "Orders" using the options pattern.
    *   Constraint: No namespaces. All types at the top level.

    *   `IOrderRepository`: `public interface IOrderRepository { }`
    *   `OrderRepository`: `public class OrderRepository : IOrderRepository { }`
    *   `IOrderService`: `public interface IOrderService { }`
    *   `OrderService`: `public class OrderService : IOrderService { }`
    *   `IEmailNotifier`: `public interface IEmailNotifier { }`
    *   `EmailNotifier`: `public class EmailNotifier : IEmailNotifier { }`
    *   `OrderSettings`: `public class OrderSettings { public string WarehouseCode { get; set; } public int MaxRetries { get; set; } }`
    *   `ServiceCollectionExtensions`:
        *   Method: `public static void AddOrderServices(thiss IServiceCollection services, IConfiguration config)`
        *   Logic:
            *   `services.AddScoped<IOrderRepository, OrderRepository>();`
            *   `services.AddScoped<IOrderService, OrderService>();`
            *   `services.AddTransient<IEmailNotifier, EmailNotifier>();`
            *   `services.Configure<OrderSettings>(config.GetSection("Orders"));`

    *   The prompt asks for a single C# file.
    *   The prompt specifies "no PackageReference for framework" in the reference, but the code requested is the logic for the service collection.
    *   The prompt asks for "no namespace".

    *   `IOrderRepository` (Interface)
    *   `OrderRepository` (Class)
    *   `IOrderService` (Interface)
    *   `OrderService` (Class)
    *   `IEmailNotifier` (Interface)
    *   `EmailNotifier` (Class)
    *   `OrderSettings` (Class)
    *   `ServiceCollectionExtensions` (Static class)

    *   `IOrderRepository` interface? Yes.
    *   `OrderRepository` class? Yes.
    *   `IOrdLService` interface? Yes.
    *   `OrderService` class? Yes.
    *   `IEmailNotifier` interface? Yes.
    *   `EmailNotifier` class? Yes.
    *   `OrderSettings` class? Yes.
    *   `ServiceCollectionExtensions` with `AddOrderServices`? Yes.
    *   Scoped/Transient registrations? Yes.
    *   Options pattern binding? Yes.
    *   No namespaces? Yes.

    *   `using Microsoft.Extensions.DependencyInjection;`
    *   `using Microsoft.Extensions.Configuration;`
    *   `using Microsoft.Extensions.s_10_11_12` (Wait, the prompt asks for a single file, and the types are defined).

    *   Wait, the prompt says "all types must be declared at the top level (global namespace)".
    *   I need to make sure the `ServiceCollectionExtensions` method uses the `IServiceCollection` and `IConfiguration` types.

    *   `IOrderRepository` (Interface)
    *   `OrderRepository` (Class)
    *   `IOrderService` (Interface)
    *’ `OrderService` (Class)
    *   `IEmailNotifier` (Interface)
    *   `EmailNotifier` (Class)
    *   `OrderSettings` (Class)
    *   `ServiceCollectionExtensions` (Static class)

    *   `public interface IOrderRepository { }`
    *   `public class OrderRepository : IOrderRepository { }`
    *   `public interface IOrderService { }`
    *   `public class OrderService : IOrderService { }`
    *   `public interface IEmailNotifier { }`
    *   `public class EmailNotifier : IEmailNotifier { }`
    *   `public class OrderSettings { public string WarehouseCode { get; set; } public int MaxRetries