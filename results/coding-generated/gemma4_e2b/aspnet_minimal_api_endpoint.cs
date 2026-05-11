using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

// 1. CreateProductRequest Record
record CreateProductRequest(string Name, decimal Price);

// 2. ProductResponse Record
record ProductResponse(int Id, string Name, decimal Price);

// 3. Static Class with Endpoint Logic
static class ProductEndpoints
{
    private static int _productIdCounter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        // --- Validation 1: Name ---
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Name", new[] { "Name is required and cannot be empty." } }
            };
            return Results.ValidationProblem(errors);
        }

        // --- Validation 2: Price ---
        if (request.Price <= 0)
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Price", new[] { "Price must be greater than zero." } }
            };
            return Results.ValidationProblem(errors);
        }

        // --- Success Path ---
        _productIdCounter++;
        var response = new ProductResponse(
            Id: _productIdCounter,
            Name: request.Name,
            Price: request.Price
        );

        return Results.Ok(response);
    }
}

// Note: In a real ASP.NET Core application, Results would be provided by the framework context.
// For this standalone exercise, we assume the existence of the Results class as defined in the API reference.
// Example placeholder for context (not required in the final output, but helpful for understanding):
/*
public static class Results
{
    public static IResult Ok(object payload) => new OkResult(200, payload);
    public static IResult ValidationProblem(Dictionary<string, string[]> errors) => new ValidationProblemResult(400, errors);
}
*/