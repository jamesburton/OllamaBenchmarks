using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

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

        var id = ++_counter;
        return Results.Ok(new ProductResponse(id, request.Name, request.Price));
    }
}