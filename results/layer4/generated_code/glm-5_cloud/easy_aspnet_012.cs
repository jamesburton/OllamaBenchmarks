using Microsoft.AspNetCore.Mvc;

public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateValidationProblem(string field, string message)
    {
        return new ProblemDetails
        {
            Status = 422,
            Title = "Validation Error",
            Detail = message,
            Extensions =
            {
                ["field"] = field
            }
        };
    }
}