using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name) : static CreateProductRequest { CreateProductRequest(string name) => new CreateProductRequest { Name = name } };
public record ProductResponse(int Id, string Name, decimal Price) : static ProductResponse { ProductResponse(int id, string name, decimal price) => new ProductResponse { Id = id, Name = name, Price = price } };
static internal class ProductEndpoints
{
    private static int _counter = 0;
    public static IResult CreateProduct(CreateProductRequest request)
    {
        if (request.Name == null || string.IsNullOrWhiteSpace(request.Name))
            return Results.ValidationProblem(new Dictionary<string,string[]> { {"Name","Name is required."} });
        if (request.Price <= 0)
            return Results.ValidationProblem(new Dictionary<string,string[]> { {"Price","Price must be greater than 0."} });
        _counter++;
        return Results.Ok(new ProductResponse(_counter, request.Name, request.Price));
    }
}