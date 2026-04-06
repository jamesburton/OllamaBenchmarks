using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;

    public record CreateProductRequest(string Name, decimal Price);

    public record ProductResponse(int Id, string Name, decimal Price);

    public static class ProductEndpoints
    {
        private static int _counter = 0;

        public static IResult CreateProduct(CreateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["Name"] = ["Name is required."] });
            }

            if (request.Price <= 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["Price"] = ["Price must be greater than 0."] });
            }

            int id = ++_counter;
            return Results.Ok(new ProductResponse(id, request.Name, request.Price));
        }
    }

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["Name"] = ["Name is required."] });
        }

        if (request.Price <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["Price"] = ["Price must be greater than 0."] });
        }

        int id = ++_counter;
        return Results.Ok(new ProductResponse(id, request.Name, request.Price));
    }
}