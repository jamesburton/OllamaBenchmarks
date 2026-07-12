public class NotFoundException : Exception
{
    public string ResourceName { get; set; }

    public NotFoundException(string resourceName) : base($"Resource not found: {resourceName}")
    {
        ResourceName = resourceName;
    }
}

public class ConflictException : Exception
{
    public string ConflictReason { get; set; }

    public ConflictException(string conflictReason) : base($"Conflict: {conflictReason}")
    {
        ConflictReason = conflictReason;
    }
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
                    Status = 404,
                    Title = "Not Found",
                    Detail = nfe.ResourceName
                };
            case ConflictException ce:
                return new ProblemDetails
                {
                    Status = 409,
                    Title = "Conflict",
                    Detail = ce.ConflictReason
                };
            default:
                return new ProblemDetails
                {
                    Status = 500,
                    Title = "Internal Server Error",
                    Detail = ex.Message
                };
        }
    }
}