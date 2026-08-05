using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);
public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _productCounter = 0;
    private static readonly object _counterLock = new();

    public static IResult CreateProduct(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> 
            { 
                ["Name"] = ["Name is required."] 
            });
        }

        if (request.Price <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> 
            { 
                ["Price"] = ["Price must be greater than zero."] 
            });
        }

        lock (_counterLock)
        {
            _productCounter++;
            var response = new ProductResponse(_productCounter, request.Name, request.Price);
            return Results.Ok(response);
        }
    }
}