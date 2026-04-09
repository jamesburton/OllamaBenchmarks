using Microsoft.AspNetCore.Mvc;

public class NotFoundException : Exception
{
    public string ResourceName { get; }

    public NotFoundException(string resourceName)
    {
        ResourceName = resourceName;
        base($"Resource not found: {resourceName}");
    }
}

public class ConflictException : Exception
{
    public string ConflictReason { get; }

    public ConflictException(string conflictReason)
    {
        ConflictReason = conflictReason;
        base($"Conflict: {conflictReason}");
    }
}

public static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex)
    {
        return ex switch
        {
            NotFoundException nex => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = nex.ResourceName
            },
            ConflictException cex => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = cex.ConflictReason
            },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = ex.Message
            }
        };
    }
}