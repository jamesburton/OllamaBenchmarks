public record Order(int Id, string Customer, decimal Total, string Status);

static class OrderProcessor
{
    public static Order MarkShipped(Order order)
        => new(order with { Status = "Shipped" });

    public static string Classify(Order order)
        return order.Total switch
        {
            > 1000m _ => "Premium",
            > 100m   => "Standard",
            _       => "Budget"
        };
}