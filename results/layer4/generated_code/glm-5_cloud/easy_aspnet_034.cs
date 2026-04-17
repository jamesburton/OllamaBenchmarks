using Microsoft.AspNetCore.Http;

public static class EndpointHandlers
{
    public static IResult GetUserById(int id, string? role)
    {
        if (id <= 0)
        {
            return Results.BadRequest("Invalid id");
        }

        if (role is not "admin" and not "user")
        {
            return Results.Forbid();
        }

        return Results.Ok(new { Id = id, Role = role });
    }
}