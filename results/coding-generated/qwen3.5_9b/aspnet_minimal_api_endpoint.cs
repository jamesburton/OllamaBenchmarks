using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public record CreateProductRequest([property: Position(0)] string Name, [property: Position(1)] decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

internal static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = ["Name is required."];
        }

        if (request.Price <= 0m)
        {
            errors["Price"] = ["Price must be greater than zero."];
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        _counter++;

        var response = new ProductResponse(_counter, request.Name, request.Price);
        return Results.Ok(response);
    }
}