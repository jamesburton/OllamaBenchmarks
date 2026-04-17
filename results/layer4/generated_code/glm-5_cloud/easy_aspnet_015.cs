using Microsoft.AspNetCore.Mvc;

public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateInternalError(string traceId)
    {
        var problemDetails = new ProblemDetails
        {
            Status = 500,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred."
        };

        problemDetails.Extensions["traceId"] = traceId;

        return problemDetails;
    }
}