using OneOf;
public class OrderItem
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public OrderItem(string product, int qty, decimal price)
        => new OrderItem(product, qty, price);
}