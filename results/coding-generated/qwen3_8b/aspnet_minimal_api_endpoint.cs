using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);
public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _idCounter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> 
            { 
                ["Name"] = new[] { "Name is required." } 
            });
        }

        if (request.Price <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> 
            { 
                ["Price"] = new[] { "Price must be greater than 0." } 
            });
        }

        _idCounter++;
        var productResponse = new ProductResponse(_idCounter, request.Name, request.Price);
        return Results.Ok(productResponse);
    }
}