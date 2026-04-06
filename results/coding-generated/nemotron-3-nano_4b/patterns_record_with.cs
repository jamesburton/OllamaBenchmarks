public record Order(int Id, string Customer, decimal Total, string Status)
{
}

static class OrderProcessor
{
    public static Order MarkShipped(Order order) => order | (o => new Order(o.Id, o.Customer, o.Total, "Shipped"));
    public static string Classify(Order order) => switch (order.Total)
    {
        >1000m => "Premium",
        >100m => "Standard",
        _ => "Budget"
    }
}