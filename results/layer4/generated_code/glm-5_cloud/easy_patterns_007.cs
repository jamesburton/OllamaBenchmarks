public class Product
{
    public string Name { get; init; }
    public decimal Price { get; init; }
}

public static class ProductFactory
{
    public static Product Create(string name, decimal price)
    {
        return new Product
        {
            Name = name,
            Price = price
        };
    }
}