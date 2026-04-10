using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Query;
using Microsoft.AspNetCore.Http.Features.Form;
using Microsoft.AspNetCore.Http.Features.Cookie;
using Microsoft.AspNetCore.Http.Features.Forwarded;
using Microsoft.AspNetCore.Http.Features.HttpRequest;
using Microsoft.AspNetCore.Http.Features.HttpContext;
using Microsoft.AspNetCore.Http.Features.Connection;
using Microsoft.AspNetCore.Http.Features.RequestHeaders;
using Microsoft.AspNetCore.Http.Features.ResponseHeaders;
using Microsoft.AspNetCore.Http.Features.RequestCookies;
using Microsoft.AspNetCore.Http.Features.ResponseCookies;
using Microsoft.AspNetCore.Http.Features.IHeaderDictionary;
using Microsoft.AspNetCore.Http.Features.IFormCollection;
using Microsoft.AspNetCore.Http.Features.IQueryCollection;
using Microsoft.AspNetCore.Http.Features.IConnectionFeature;
using Microsoft.AspNetCore.Http.Features.IHttpContextFeature;
using Microsoft.AspNetCore.Http.Features.IRequestFeature;
using Microsoft.AspNetCore.Http.Features.IResponseFeature;
using Microsoft.AspNetCore.Http.Features.IConnectionFeature;

// 1. CreateProductRequest record
public record CreateProductRequest(string Name, decimal Price);

// 2. ProductResponse record
public record ProductResponse(int Id, string Name, decimal Price);

// 3. ProductEndpoints static class
public static class ProductEndpoints
{
    private static int _nextId = 0;

    public static IResult CreateProduct(CreateProductRequest request)
    {
        var validationErrors = new Dictionary<string, string[]>();

        // Validate Name (required, must be non-empty)
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            validationErrors["Name"] = new[] { "Name is required and cannot be empty." };
        }

        // Validate Price (must be > 0)
        if (request.Price <= 0)
        {
            validationErrors["Price"] = new[] { "Price must be greater than zero." };
        }

        // Check if any validation errors occurred
        if (validationErrors.Count > 0)
        {
            // Return ValidationProblem if validation fails
            return Results.ValidationProblem(validationErrors);
        }

        // Success path
        _nextId++;
        var response = new ProductResponse(_nextId, request.Name, request.Price);

        // Return Ok(payload)
        return Results.Ok(response);
    }
}