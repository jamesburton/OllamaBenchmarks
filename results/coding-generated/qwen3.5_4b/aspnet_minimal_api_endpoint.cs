using Microsoft.AspNetCore.Http;

    public record CreateProductRequest(string Name, decimal Price);
    public record ProductResponse(int Id, string Name, decimal Price);

    public static class ProductEndpoints
    {
        private static int _counter = 0;

        public static IResult CreateProduct(CreateProductRequest request)
        {
            // Validation logic here
        }
    }

... 
using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) || request.Name.Trim().Length == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Name"] = new[] { "Name is required." }
            });
        }

        if (request.Price <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Price"] = new[] { "Price must be greater than zero." }
            });
        }

        _counter++;
        return Results.Ok(new ProductResponse(_counter, request.Name, request.Price));
    }
}

public record CreateProductRequest(string Name, decimal Price);

using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) || request.Name.Trim().Length == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Name"] = new[] { "Name is required." }
            });
        }

        if (request.Price <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Price"] = new[] { "Price must be greater than zero." }
            });
    }

    _counter++;
    return Results.Ok(new ProductResponse(_counter, request.Name, request.Price));
}

public static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        // ... validation ...
        // ... success ...
    }
}