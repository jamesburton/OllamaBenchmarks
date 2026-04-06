public record Order(int Id, string Customer, decimal Total, string Status);  
static class OrderProcessor {  
    static string Classify(Order order) {  
        switch(order.Total) {  
            case decimal.Total > 1000m: return "Premium";  
            case decimal.Total > 100m: return "Standard";  
            default: return "Budget";  
        }  
    }  
}