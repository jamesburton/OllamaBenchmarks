public record Order(int Id, string Customer, decimal Total, string Status)
{
}
public static class OrderProcessor
{
    public static Order MarkShipped(Order order) => order with { Status = "Shipped"; };

    public static string Classify(Order order) => order.Total switch when (order.Total > 1000m) => "Premium" when (order.Total > 100m) => "Standard" => "Budget";
}