using Microsoft.AspNetCore.Http.HttpResults;

public static class EndpointHandlers
{
    public static IResult SearchItems(string? query)
    {
        if (query is not null && query.Length < 3)
        {
            return Results.BadRequest("Query too short");
        }

        return Results.Ok(new { Query = query, Results = Array.Empty<string>() });
    }
}