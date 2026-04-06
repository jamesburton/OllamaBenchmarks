using Microsoft.AspNetCore.Http;

record CreateProductRequest(string Name, decimal Price);

record ProductResponse(int Id, string Name, decimal Price);

public static class ProductEndpoints
{
    private static int _idCounter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            var errors = new Dictionary<string, string[]> { ["Name"] = ["Name is required."] };
            return Results.ValidationProblem(errors);
        }

        if (request.Price <= 0)
        {
            var errors = new Dictionary<string, string[]> { ["Price"] = ["Price must be greater than zero."] };
            return Results.ValidationProblem(errors);
        }

        _idCounter++;
        var productResponse = new ProductResponse(Id: _idCounter, Name: request.Name, Price: request.Price);
        return Results.Ok(productResponse);
    }
}