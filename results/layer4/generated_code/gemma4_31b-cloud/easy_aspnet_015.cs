using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateInternalError(string traceId)
    {
        var problemDetails = new ProblemDetails
        {
            Status = 500,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred.",
            Extensions = new Dictionary<string, object>
            {
                { "traceId", traceId }
            }
        };

        return problemDetails;
    }
}