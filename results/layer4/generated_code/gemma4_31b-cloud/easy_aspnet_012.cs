using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateValidationProblem(string field, string message)
    {
        return new ProblemDetails
        {
            Status = 422,
            Title = "Validation Error",
            Detail = message,
            Extensions = new Dictionary<string, object>
            {
                { "field", field }
            }
        };
    }
}