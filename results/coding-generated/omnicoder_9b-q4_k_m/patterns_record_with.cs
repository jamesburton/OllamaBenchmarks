public record Order(int Id, string Customer, decimal Total, string Status);

public static class OrderProcessor
        {
            // Method a
            public static Order MarkShipped(Order order)
            {
                return order with { Status = "Shipped" };
            }

            // Method b
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

return order.Total switch
    {
        > 1000m => "Premium",
        > 100m => "Standard",
        _ => "Budget"
    };

public record Order(int Id, string Customer, decimal Total, string Status);

public record Order(int Id, string Customer, decimal Total, string Status);

    public static class OrderProcessor
    {
        public static Order MarkShipped(Order order)
        {
            return order with { Status = "Shipped" };
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

public record Order(int Id, string Customer, decimal Total, string Status);

    public static class OrderProcessor
    {
        public static Order MarkShipped(Order order)
        {
            return order with { Status = "Shipped" };
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