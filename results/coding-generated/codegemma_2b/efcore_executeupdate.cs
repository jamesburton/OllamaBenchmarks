public class Product {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

public class ProductDbContext extends DbContext {
    public ProductDbContext(DbContextOptions<ProductDbContext> options) {
        super(options);
    }

    public