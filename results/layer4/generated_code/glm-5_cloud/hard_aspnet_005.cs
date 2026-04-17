using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

// 1. Records
public record CreateOrderRequest(
    string CustomerId, 
    [property: JsonConverter(typeof(JsonConverterForIReadOnlyList))]
    IReadOnlyList<OrderLineRequest> Lines);

public record OrderLineRequest(string ProductId, int Quantity, decimal UnitPrice);

public record OrderResponse(Guid Id, string CustomerId, decimal Total, DateTime CreatedAt);

// 2. Validator
public class OrderValidator
{
    public ValidationResult Validate(CreateOrderRequest req)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(req.CustomerId))
        {
            errors.Add(new ValidationError("CustomerId", "CustomerId is required."));
        }

        if (req.Lines == null || req.Lines.Count == 0)
        {
            errors.Add(new ValidationError("Lines", "Lines must contain at least one item."));
        }
        else
        {
            for (int i = 0; i < req.Lines.Count; i++)
            {
                var line = req.Lines[i];
                if (line.Quantity <= 0)
                {
                    errors.Add(new ValidationError($"Lines[{i}].Quantity", "Quantity must be greater than 0."));
                }
                if (line.UnitPrice <= 0)
                {
                    errors.Add(new ValidationError($"Lines[{i}].UnitPrice", "UnitPrice must be greater than 0."));
                }
            }
        }

        return new ValidationResult(errors);
    }
}

public record ValidationError(string Field, string Message);

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationResult(IReadOnlyList<ValidationError> errors)
    {
        Errors = errors;
    }
}

// 3. Interface and Service
public interface IOrderService
{
    Task<OrderResponse> PlaceOrderAsync(CreateOrderRequest req, CancellationToken ct);
    Task<OrderResponse?> GetByIdAsync(Guid id);
}

public class InMemoryOrderService : IOrderService
{
    private readonly Dictionary<Guid, OrderResponse> _orders = new();

    public Task<OrderResponse> PlaceOrderAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var total = req.Lines.Sum(l => l.Quantity * l.UnitPrice);
        var order = new OrderResponse(Guid.NewGuid(), req.CustomerId, total, DateTime.UtcNow);
        _orders[order.Id] = order;
        return Task.FromResult(order);
    }

    public Task<OrderResponse?> GetByIdAsync(Guid id)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }
}

// 4. Endpoints
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/orders");

        group.MapPost("/", async (CreateOrderRequest req, IOrderService service, CancellationToken ct) =>
        {
            var validator = new OrderValidator();
            var result = validator.Validate(req);

            if (!result.IsValid)
            {
                var problemDetails = new HttpValidationProblemDetails(
                    result.Errors.ToDictionary(e => e.Field, e => new[] { e.Message })
                )
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Validation Failed",
                    Type = "https://httpstatuses.com/422"
                };

                return Results.Problem(problemDetails);
            }

            var order = await service.PlaceOrderAsync(req, ct);
            return Results.Created($"/orders/{order.Id}", order);
        });

        group.MapGet("/{id:guid}", async (Guid id, IOrderService service) =>
        {
            var order = await service.GetByIdAsync(id);
            return order is not null ? Results.Ok(order) : Results.NotFound();
        });
    }
}

// Helper for JSON serialization of IReadOnlyList (Minimal API binding handles List/Array automatically, 
// but this ensures the record definition matches the prompt exactly).
internal class JsonConverterForIReadOnlyList : JsonConverter<IReadOnlyList<OrderLineRequest>>
{
    public override IReadOnlyList<OrderLineRequest>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<List<OrderLineRequest>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<OrderLineRequest> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}