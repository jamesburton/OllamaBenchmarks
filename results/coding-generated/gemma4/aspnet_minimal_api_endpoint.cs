using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;

// 1. CreateProductRequest record
public record CreateProductRequest(string Name, decimal Price);

// 2. ProductResponse record
public record ProductResponse(int Id, string Name, decimal Price);

// 3. ProductEndpoints static class
public static class ProductEndpoints
{
    private static int _nextId = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        // Validation for Name (null or whitespace)
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Name", new[] { "Name is required and cannot be empty." } }
            };
            return Results.ValidationProblem(errors);
        }

        // Validation for Price (must be > 0)
        if (request.Price <= 0)
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Price", new[] { "Price must be greater than zero." } }
            };
            return Results.ValidationProblem(errors);
        }

        // Success path
        _nextId++;
        var response = new ProductResponse(_nextId, request.Name, request.Price);
        return Results.Ok(response);
    }
}