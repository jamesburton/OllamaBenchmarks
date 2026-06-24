using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int