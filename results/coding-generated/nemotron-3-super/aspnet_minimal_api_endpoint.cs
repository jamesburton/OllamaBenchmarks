using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Http;

record CreateProductRequest(string Name, decimal Price);
record ProductResponse(int Id, string Name, decimal Price);

static class ProductEndpoints
{
    private static int _nextId = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name))
            errors["Name"] = new[] { "Name is required." };
        if (request.Price <= 0)
            errors["Price"] = new[] { "Price must be greater than zero." };
        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        var id = Interlocked.Increment(ref _nextId);
        return Results.Ok(new ProductResponse(id, request.Name, request.Price));
    }
}