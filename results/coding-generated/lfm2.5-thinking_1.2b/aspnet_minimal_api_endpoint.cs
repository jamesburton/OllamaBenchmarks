using System;
using Microsoft.AspNetCore.Http;

private static int counter = 0;

public static class ProductEndpoints
{
    private static int counter = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var name = request.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ValidationProblem(new Dictionary<string, string[]> { ["Name"] = null });
        }
        if (request.Price <= 0)
        {
            return new ValidationProblem(new Dictionary<string, string[]> { ["Name"] = null });
        }
        counter++;
        return new ProductResponse
        {
            Id = counter,
            Name = name,
            Price = request.Price
        };
    }
}