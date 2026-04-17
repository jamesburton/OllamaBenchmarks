using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

public static class EndpointHandlers
{
    public static IResult GetStats(int[] values)
    {
        if (values == null || values.Length == 0)
        {
            return Results.BadRequest("No values provided");
        }

        return Results.Ok(new 
        { 
            Min = values.Min(), 
            Max = values.Max(), 
            Avg = values.Average() 
        });
    }
}