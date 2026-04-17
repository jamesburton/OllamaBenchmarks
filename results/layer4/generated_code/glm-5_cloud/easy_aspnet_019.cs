using Microsoft.AspNetCore.Http.HttpResults;

public static class ApiResponseHelper
{
    public static IResult ToResult<T>(T? value) where T : class
    {
        if (value is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(value);
    }
}