using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Name is required." };
            return Results.ValidationProblem(errors);
        }

        if (request.Price <= 0)
        {
            errors["Price"] = new[] { "Price must be greater than zero." };
            return Results.ValidationProblem(errors);
        }

        _counter++;
        var product = new ProductResponse(_counter, request.Name, request.Price);
        return Results.Ok(product);
    }
}