using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public static class ApiResponseHelper
{
    public static IResult ToResult<T>(T? value) where T : class
    {
        return value == null ? Results.NotFound() : Results.Ok(value);
    }
}