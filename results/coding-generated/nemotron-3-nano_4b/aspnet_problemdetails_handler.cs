using Microsoft.AspNetCore.Mvc;

public class NotFoundException : Exception
{
    public string ResourceName { get; }

    public NotFoundException(string resourceName) : base($"Resource not found: {resourceName}") => ResourceName = resourceName;
}

public class ConflictException : Exception
{
    public string ConflictReason { get; }

    public ConflictException(string conflictReason) : base($"Conflict: {conflictReason}") => ConflictReason = conflictReason;
}

public static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex)
    {
        switch (ex)
        {
            case NotFoundException nfe:
                return new ProblemDetails
                {
                    Status = StatusCodes.Status404,
                    Title = "Not Found",
                    Detail = nfe.ResourceName
                };

            case ConflictException ce:
                return new ProblemDetails
                {
                    Status = StatusCodes.Status409,
                    Title = "Conflict",
                    Detail = ce.ConflictReason
                };

            default:
                return new ProblemDetails
                {
                    Status = StatusCodes.Status500,
                    Title = "Internal Server Error",
                    Detail = ex.Message
                };
        }
    }
}