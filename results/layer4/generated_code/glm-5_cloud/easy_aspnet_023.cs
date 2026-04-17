using System;
using System.Collections.Generic;

public class ProblemDetails
{
    public int Status { get; set; }
    public string Title { get; set; }
    public string Detail { get; set; }
}

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