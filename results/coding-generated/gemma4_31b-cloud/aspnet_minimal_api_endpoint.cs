using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

public record CreateProductRequest([Required] string Name, [Range(0.01, double.MaxValue)] decimal Price);

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
                ["Name"] = ["Name is required."] 
            });
        }

        if (request.Price <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> 
            { 
                ["Price"] = ["Price must be greater than 0."] 
            });
        }

        int id = ++_idCounter;
        return Results.Ok(new ProductResponse(id, request.Name, request.Price));
    }
}