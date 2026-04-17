using Microsoft.AspNetCore.Mvc;

public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateUnauthorized(string detail)
    {
        return new ProblemDetails
        {
            Status = 401,
            Title = "Unauthorized",
            Detail = detail
        };
    }
}