using Microsoft.AspNetCore.Http;

// Global namespace (no namespace declaration)

record CreateProductRequest(string Name, decimal Price);

record ProductResponse(int Id, string Name, decimal Price);

static class ProductEndpoints
{
    private static int s_idCounter = 0;

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

        s_idCounter++;
        int id = s_idCounter;
        var response = new ProductResponse(id, request.Name, request.Price);
        return Results.Ok(response);
    }
}