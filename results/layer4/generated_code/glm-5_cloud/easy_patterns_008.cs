public class Order
{
    public required string OrderId { get; set; }
    public required int Quantity { get; set; }
}

public static class OrderFactory
{
    public static Order Create(string orderId, int quantity)
    {
        return new Order
        {
            OrderId = orderId,
            Quantity = quantity
        };
    }
}