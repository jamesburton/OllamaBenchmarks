using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

public static class EndpointHandlers
{
    public static IResult GetVersion()
    {
        return Results.Ok(new { Version = "1.0.0", BuildDate = "2025-01-01" });
    }
}