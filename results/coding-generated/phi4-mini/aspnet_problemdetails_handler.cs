using System;
using Microsoft.AspNetCore.Http;
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
    public static ProblemDetails MapException(Exception ex)
    {
        return ex switch
        {
            NotFoundException notFoundEx => new ProblemDetails
            (
                title: "Not Found",
                statusCode: 404,
                detail: notFoundEx.ResourceName
            ),
            ConflictException conflictEx => new ProblemDetails
            (
                title: "Conflict",
                statusCode: 409,
                detail: conflictEx.ConflictReason
            ),
            _ => new ProblemDetails
            (
                title: "Internal Server Error",
                statusCode: 500,
                detail: ex.Message
            )
        };
    }
}