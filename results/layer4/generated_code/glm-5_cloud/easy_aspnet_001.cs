using Microsoft.AspNetCore.Http.HttpResults;

public static class EndpointHandlers
{
    public static IResult GetPing()
    {
        return Results.Ok("pong");
    }
}