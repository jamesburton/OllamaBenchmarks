using Microsoft.AspNetCore.Http;

record CreateProductRequest(string Name, decimal Price);
record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Name is required." };
        }

        if (request.Price <= 0)
        {
            errors["Price"] = new[] { "Price must be greater than 0." };
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        _counter++;
        return Results.Ok(new ProductResponse(_counter, request.Name, request.Price));
    }
}