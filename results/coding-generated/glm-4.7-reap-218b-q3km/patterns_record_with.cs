public record Order(int Id, string Customer, decimal Total, string Status);

public static class OrderProcessor
{
    public static Order MarkShipped(Order order)
    {
        return order with { Status = "Shipped" };
    }

    public static string Classify(Order order)
    {
        return switch (order.Total)
        {
            case > 1000m => "Premium",
            case > 100m => "Standard",
            _ => "Budget"
        };
    }
}