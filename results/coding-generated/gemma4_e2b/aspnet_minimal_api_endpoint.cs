using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Results;
using System.Collections.Generic;

// 1. CreateProductRequest record
record CreateProductRequest(string Name, decimal Price);

// 2. ProductResponse record
record ProductResponse(int Id, string Name, decimal Price);

// 3. Static class ProductEndpoints
public static class ProductEndpoints
{
    private static int _productIdCounter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        // Validation for Name
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("Name", new[] { "Name is required." });
        }

        // Validation for Price
        if (request.Price <= 0)
        {
            errors.Add("Price", new[] { "Price must be greater than 0." });
        }

        if (errors.Count > 0)
        {
            // Return validation errors if validation fails
            return Results.ValidationProblem(errors);
        }

        // Success case
        _productIdCounter++;
        var response = new ProductResponse(_productIdCounter, request.Name, request.Price);
        return Results.Ok(response);
    }
}