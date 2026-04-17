using Microsoft.AspNetCore.Mvc;

public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateConflict(string resource, string detail)
    {
        return new ProblemDetails
        {
            Status = 409,
            Title = "Conflict",
            Detail = detail
        };
        // Note: The 'resource' parameter is accepted but not used in the returned object
        // as the requirement to add it to Extensions was removed.
    }
}