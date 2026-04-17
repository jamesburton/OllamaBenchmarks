using Microsoft.AspNetCore.Http;

public static class EndpointHandlers
{
    public static IResult GetById(int id)
    {
        if (id <= 0)
        {
            return Results.NotFound();
        }

        return Results.Ok(new { Id = id, Name = $"Item {id}" });
    }
}