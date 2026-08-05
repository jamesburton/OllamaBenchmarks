using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price)
{
    return new ProductRequest()
        .Name = Name
        .Price = Price;
}

public record ProductResponse(int Id, string Name, decimal Price)
{
    return new ProductResponse()
        .Id = Id
        .Name = Name
        .Price = Price;
}

public static IResult CreateProduct(CreateProductRequest request)
{
    var validationErrors = new Dictionary<string, string[]>();
    if (!string.IsNullOrEmpty(request.Name))
    {
        validationErrors["Name"] = "Name is required.";
    }
    else
    {
        validationErrors["Name"] = "Name is required.";
    }

    if (request.Price <= 0)
    {
        validationErrors["Price"] = "Price must be greater than zero.";
    }

    if (!validationErrors.Any())
    {
        var counter = ProductEndpoints.Counter++;
        return Results Ok(new ProductResponse(counter, request.Name, request.Price));
    }
    else
    {
        return Results ValidationProblem(validationErrors);
    }
}