using System;
using Microsoft.AspNetCore.Mvc;

public static class ResponseMapper
{
    public static ProblemDetails FromException(Exception ex)
    {
        int status = ex switch
        {
            ArgumentException => 400,
            KeyNotFoundException => 404,
            _ => 500
        };

        return new ProblemDetails
        {
            Status = status,
            Title = ex.GetType().Name,
            Detail = ex.Message
        };
    }
}