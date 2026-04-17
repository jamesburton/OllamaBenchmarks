using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;

public static class EndpointHandlers
{
    public static IResult GetList(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1)
        {
            return Results.BadRequest("Invalid pagination");
        }

        return Results.Ok(new { Page = page, PageSize = pageSize, Items = Array.Empty<string>() });
    }
}