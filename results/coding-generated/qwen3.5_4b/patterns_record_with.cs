public record Order(int Id, string Customer, decimal Total, string Status);

public static class OrderProcessor
{
    public static Order MarkShipped