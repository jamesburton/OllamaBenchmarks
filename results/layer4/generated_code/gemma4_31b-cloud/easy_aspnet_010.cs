using Microsoft.AspNetCore.Http;

public static class EndpointHandlers
{
    public static IResult DeleteItem(int id, bool exists)
    {
        if (exists)
        {
            return Results.NoContent();
        }

        return Results.NotFound();
    }
}