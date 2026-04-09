using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

record CreateProductRequest(string Name, decimal Price);
record ProductResponse(int Id, string Name, decimal Price);

static class ProductEndpoints
{
    private static int _idCounter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("Name", ["Name is required."]);
        }

        if (request.Price <= 0)
        {
            errors.Add("Price", ["Price must be greater than 0."]);
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        _idCounter++;
        var response = new ProductResponse(_idCounter, request.Name, request.Price);
        return Results.Ok(response);
    }
}