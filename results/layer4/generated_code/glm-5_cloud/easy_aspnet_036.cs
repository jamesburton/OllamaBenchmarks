using Microsoft.AspNetCore.Http;

public static class EndpointHandlers
{
    public static IResult GetContent(string format)
    {
        if (format == "html")
        {
            return Results.Content("<html>Hello</html>", "text/html");
        }

        if (format == "json")
        {
            return Results.Content("{\"message\":\"Hello\"}", "application/json");
        }

        return Results.BadRequest("Unknown format");
    }
}