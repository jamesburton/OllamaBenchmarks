public record Order(int Id, string Customer, decimal Total, string Status);

public static class OrderProcessor
{
    public static Order MarkShipped(Order order)
    {
        return order with { Status = "Shipped" };
    }

    public static string Classify(Order order)
    {
        return order.Total switch
        {
            var t when t > 1000m => "Premium",
            var t when t > 100m  => "Standard",
            _                    => "Budget"
        };
    }
}