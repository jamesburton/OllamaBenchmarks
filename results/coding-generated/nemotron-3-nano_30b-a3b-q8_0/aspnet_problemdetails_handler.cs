using Microsoft.AspNetCore.Mvc;

public class NotFoundException : Exception
{
    public string ResourceName { get; }
    public NotFoundException(string resourceName) : base($"Resource not found: {resourceName}")
    {
        ResourceName = resourceName;
    }
}

public class ConflictException : Exception
{
    public string ConflictReason { get; }
    public ConflictException(string conflictReason) : base($"Conflict: {conflictReason}")
    {
        ConflictReason = conflictReason;
    }
}

public static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex) => ex switch
    {
        NotFoundException ne => new ProblemDetails
        {
            Status = 404,
            Title = "Not Found",
            Detail = ne.ResourceName
        },
        ConflictException ce => new ProblemDetails
        {
            Status = 409,
            Title = "Conflict",
            Detail = ce.ConflictReason
        },
        _ => new ProblemDetails
        {
            Status = 500,
            Title = "Internal Server Error",
            Detail = ex.Message
        }
    };
}