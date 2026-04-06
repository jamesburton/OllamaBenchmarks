using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints





private static int _idCounter = 0;


public static IResult CreateProduct(CreateProductRequest request)