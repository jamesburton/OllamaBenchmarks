using Microsoft.AspNetCore.Http.HttpResults;

public static class EndpointHandlers
{
    public static IResult DeleteItem(int id, bool exists)
    {
        return exists ? Results.NoContent() : Results.NotFound();
    }
}