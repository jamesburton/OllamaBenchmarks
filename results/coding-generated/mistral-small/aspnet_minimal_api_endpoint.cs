using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var validationErrors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            validationErrors["Name"] = new[] { "Name is required." };
        }

        if (request.Price <= 0)
        {
            validationErrors["Price"] = new[] { "Price must be greater than zero." };
        }

        if (validationErrors.Any())
        {
            return Results.ValidationProblem(validationErrors);
        }

        counter++;
        var productResponse = new ProductResponse(counter, request.Name, request.Price);

        return Results.Ok(productResponse);
    }
}