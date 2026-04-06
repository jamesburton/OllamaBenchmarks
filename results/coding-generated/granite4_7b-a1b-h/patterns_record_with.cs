public record Order(int Id, string Customer, decimal Total, string Status);
static class OrderProcessor {
    public static Order MarkShipped(Order order) => new(order.Id, order.Customer, order.Total, "Shipped");
    public static string Classify(Order order) => order.Total > 1000m ? "Premium" : (order.Total > 100m ? "Standard" : "Budget");
}