using Microsoft.AspNetCore.Http.HttpResults;

public static class EndpointHandlers
{
    public static IResult CreateItem(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Results.BadRequest("Name is required");
        }

        return Results.Created($"/items/{name}", new { Name = name });
    }
}