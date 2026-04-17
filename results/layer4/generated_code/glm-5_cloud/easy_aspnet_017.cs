using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

public static class EndpointHandlers
{
    public static IResult Redirect(string url)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith("http"))
        {
            return Results.BadRequest("Invalid URL");
        }

        return Results.Redirect(url);
    }
}