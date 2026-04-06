using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;

public record CreateProductRequest(string Name, decimal Price);

public record ProductResponse(int Id, string Name, decimal Price);

public static class Results {
    public static IResult Ok(object payload) => new Result(HttpStatusCode.OK);

    public static IResult Created(string path, object obj) => new Result(HttpStatusCode.Created, path);

    public static IResult NoContent() => new Result(HttpStatusCode.NoContent);

    public static IResult NotFound() => new Result(HttpStatusCode.NotFound);

    public static IResult BadRequest(string msg) => new Result(HttpStatusCode.BadRequest, msg);

    public static IResult Problem(object detail, HttpStatusCode statusCode = 500) =>
        new Result(statusCode, JsonSerializer.Serialize(detail));
}

public interface IResult {
    void WriteAsJsonAsync(HttpResponseMessage response);
}

public class Result : IResult {
    private readonly int _status;
    private readonly string? _path;
    private readonly object? _payload;

    public Result(int status) => _status = status;

    public Result(string path, HttpStatusCode status) => { _path = path; _status = status; }

    public void WriteAsJsonAsync(HttpResponseMessage response) {
        var content = new StringContent(JsonSerializer.Serialize(_payload), System.Text.Encoding.UTF8, "application/json");
        response.Content?.Write(content);
    }

    private Result(int status) => StatusCode = (HttpStatusCode)status;
}

public static class ProductEndpoints {
    public static int Counter { get; set; } = 0;

    public static IResult CreateProduct(CreateProductRequest request) {
        if (string.IsNullOrWhiteSpace(request.Name)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["Name"] = new[] { "Name is required." } });

        if (request.Price <= 0) return Results.ValidationProblem(new Dictionary<string, string[]> { ["Price"] = new[] { "Price must be greater than zero." } });

        Counter++;
        var id = Interlocked.Increment(ref Counter);
        return Results.Ok(new ProductResponse(id, request.Name, request.Price));
    }
}