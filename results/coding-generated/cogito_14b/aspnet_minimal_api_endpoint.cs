using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _nextId = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Name is required." };
        }

        if (request.Price <= 0)
        {
            errors["Price"] = new[] { "Price must be greater than zero." };
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var id = ++_nextId;
        var response = new ProductResponse(id, request.Name, request.Price);
        return Results.Ok(response);
    }
}