public record Order(int Id, string Customer, decimal Total, string Status);

public static class OrderProcessor
{
    public static Order MarkShipped(Order order) => 
        new(order.Id, order.Customer, order.Total, "Shipped");

    public static string Classify(Order order) =>
        order.Total switch
        {
            > 1000m => "Premium",
            > 100m  => "Standard",
            _       => "Budget"
        };
}