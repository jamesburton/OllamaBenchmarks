using Microsoft.AspNetCore.Mvc;

public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateNotFound(string detail)
    {
        return new ProblemDetails
        {
            Status = 404,
            Title = "Not Found",
            Detail = detail
        };
    }
}