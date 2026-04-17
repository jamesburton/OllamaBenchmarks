using Microsoft.AspNetCore.Http;

public static class EndpointHandlers
{
    public static IResult GetPing()
    {
        return Results.Ok("pong");
    }
}