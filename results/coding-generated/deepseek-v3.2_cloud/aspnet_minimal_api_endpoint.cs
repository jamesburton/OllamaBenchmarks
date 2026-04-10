using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _idCounter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Name is required and cannot be empty or whitespace." };
        }

        if (request.Price <= 0)
        {
            errors["Price"] = new[] { "Price must be greater than 0." };
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var newId = Interlocked.Increment(ref _idCounter);
        var response = new ProductResponse(newId, request.Name, request.Price);

        return Results.Ok(response);
    }
}