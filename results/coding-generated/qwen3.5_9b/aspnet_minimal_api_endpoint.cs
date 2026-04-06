using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

[Required]
public record CreateProductRequest([property: Positional] string Name, [property: Positional] decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = ["Name is required."];
        }

        if (request.Price <= 0)
        {
            errors["Price"] = ["Price must be greater than zero."];
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        _counter++;
        var id = _counter;
        return Results.Ok(new ProductResponse(id, request.Name, request.Price));
    }
}