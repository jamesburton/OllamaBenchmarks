using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;

// 1. CreateProductRequest record
public record CreateProductRequest(string Name, decimal Price);

// 2. ProductResponse record
public record ProductResponse(int Id, string Name, decimal Price);

// 3. Static class containing the endpoint logic
public static class ProductEndpoints
{
    // Private static counter for IDs
    private static int _idCounter = 0;

    /// <summary>
    /// Creates a new product, performing validation checks.
    /// </summary>
    /// <param name="request">The product creation request.</param>
    /// <returns>An IResult representing the HTTP response.</returns>
    public static IResult CreateProduct(CreateProductRequest request)
    {
        // --- Validation Checks ---

        // 1. Validate Name (null or whitespace)
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            var errors = new Dictionary<string, string[]>
            {
                ["Name"] = new[] { "Name is required and cannot be empty." }
            };
            return Results.ValidationProblem(errors);
        }

        // 2. Validate Price (must be > 0)
        if (request.Price <= 0)
        {
            var errors = new Dictionary<string, string[]>
            {
                ["Price"] = new[] { "Price must be greater than zero." }
            };
            return Results.ValidationProblem(errors);
        }

        // --- Success Path ---

        // Increment counter and generate ID
        _idCounter++;
        int newId = _idCounter;

        // Create response object
        var response = new ProductResponse(newId, request.Name, request.Price);

        // Return 200 OK with the created product details
        return Results.Ok(response);
    }
}