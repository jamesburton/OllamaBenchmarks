using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateConflict(string resource, string detail)
    {
        return new ProblemDetails
        {
            Status = 409,
            Title = "Conflict",
            Detail = detail,
            Extensions = new Dictionary<string, object>
            {
                { "resource", resource }
            }
        };
    }
}