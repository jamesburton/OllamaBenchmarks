using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

public static class EndpointHandlers
{
    public static IResult GetHealth(bool isHealthy)
    {
        if (isHealthy)
        {
            return Results.Ok(new { Status = "healthy" });
        }

        return Results.Json(
            new { Status = "unhealthy" }, 
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
}