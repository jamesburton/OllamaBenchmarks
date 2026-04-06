using Microsoft.AspNetCore.Mvc;

public sealed class NotFoundException : Exception
{
    public string ResourceName { get; }
    public NotFoundException(string resourceName) : base($"Resource not found: {resourceName}") { ResourceName = resourceName; }
}

public sealed class ConflictException : Exception
{
    public string ConflictReason { get; }
    public ConflictException(string conflictReason) : base($"Conflict: {conflictReason}") { ConflictReason = conflictReason; }
}

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex)
    {
        return switch (ex)
        {
            case NotFoundException nfe when nfe.ResourceName != null: new ProblemDetails { Title = "Not Found", Status = 404, Detail = nfe.ResourceName },
            case ConflictException cef when cef.ConflictReason != null: new ProblemDetails { Title = "Conflict", Status = 409, Detail = cef.ConflictReason },
            default: new ProblemDetails { Title = "Internal Server Error", Status = 500, Detail = ex.Message }
        };
    }
}