using Microsoft.AspNetCore.Mvc;

public class NotFoundException : Exception
{
    public string ResourceName { get; }

    public NotFoundException(string resourceName)
        : base($"Resource not found: {resourceName}")
    {
        ResourceName = resourceName;
    }
}

public class ConflictException : Exception
{
    public string ConflictReason { get; }

    public ConflictException(string conflictReason)
        : base($"Conflict: {conflictReason}")
    {
        ConflictReason = conflictReason;
    }
}

public static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex)
    {
        return ex switch
        {
            NotFoundException nfe => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = nfe.ResourceName
            },
            ConflictException ce => new ProblemDetails
            {