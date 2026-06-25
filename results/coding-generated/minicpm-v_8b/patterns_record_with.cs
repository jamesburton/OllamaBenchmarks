public record Order(int Id, string Customer, decimal Total, string Status);

static class OrderProcessor
{
    public static Order MarkShipped(Order order)
    {
        return new Order(order.Id, order.Customer, order.Total, "Shipped");
    }

    public static string Classify(Order order)
    {
        switch (order.Total)
        {
            case _ when order.Total > 1000:
                return "Premium";
            case _ when order.Total > 100:
                return "Standard";
            default:
                return "Budget";
        }
    }
}