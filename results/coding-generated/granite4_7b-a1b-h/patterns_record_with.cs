public record Order(int Id, string Customer, decimal Total, string Status);
static class OrderProcessor
{
    public static Order MarkShipped(Order order)
    {
        return new Order(order.Id, order.Customer, order.Total, "Shipped");
    }

    public static string Classify(Order order)
    {
        return order.Total switch
        {
            > 1000m => "Premium",
            > 100m => "Standard",
            _ => "Budget"
        };
    }
}