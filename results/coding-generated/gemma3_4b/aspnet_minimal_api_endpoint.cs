using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _productCounter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                { "Name", new string[] { "Name is required." } }
            });
        }

        if (request.Price <= 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                { "Price", new string[] { "Price must be greater than 0." } }
            });
        }

        _productCounter++;
        int id = _productCounter;

        return Results.Ok(new ProductResponse(id, request.Name, request.Price));
    }
}