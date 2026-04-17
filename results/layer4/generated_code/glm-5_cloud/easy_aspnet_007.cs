using Microsoft.AspNetCore.Mvc;

public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateBadRequest(string detail)
    {
        return new ProblemDetails
        {
            Status = 400,
            Title = "Bad Request",
            Detail = detail
        };
    }
}