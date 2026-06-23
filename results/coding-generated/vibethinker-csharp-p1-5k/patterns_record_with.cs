public record Order(int Id, string Customer, decimal Total, string Status);

static Order MarkShipped(Order order)
{
            return order.with(Status => "Shipped");
        }

string Classify(Order order)
{
            switch (order.Total)
            {
                case > 1000m:
                    return "Premium";
                case > 100m:
                    return "Standard";
                default:
                    return "Budget";
            }
        }