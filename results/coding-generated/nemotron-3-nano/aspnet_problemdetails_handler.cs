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
    public static ProblemDetails MapException(Exception ex) =>
        ex switch
        {
            NotFoundException nfe => new ProblemDetails
            {
                Title = "Not Found",
                Status = 404,
                Detail = nfe.ResourceName
            },

            ConflictException ce => new ProblemDetails
            {
                Title = "Conflict",
                Status = 409,
                Detail = ce.ConflictReason
            },

            _ => new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = 500,
                Detail = ex.Message
            }
        };
}