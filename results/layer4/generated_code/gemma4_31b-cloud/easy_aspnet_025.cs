using Microsoft.AspNetCore.Http;

public static class EndpointHandlers
{
    public static IResult UpdateItem(int id, string? name)
    {
        if (id <= 0)
        {
            return Results.BadRequest("Invalid id");
        }

        if (string.IsNullOrEmpty(name))
        {
            return Results.BadRequest("Name required");
        }

        return Results.Ok(new { Id = id, Name = name });
    }
}