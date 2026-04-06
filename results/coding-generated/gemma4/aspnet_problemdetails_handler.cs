using Microsoft.AspNetCore.Mvc;
using System;

// 1. NotFoundException
public class NotFoundException : Exception
{
    public string ResourceName { get; }

    public NotFoundException(string resourceName) : base($"Resource not found: {resourceName}")
    {
        ResourceName = resourceName;
    }
}

// 2. ConflictException
public class ConflictException : Exception
{
    public string ConflictReason { get; }

    public ConflictException(string conflictReason) : base($"Conflict: {conflictReason}")
    {
        ConflictReason = conflictReason;
    }
}

// 3. ApiExceptionMapper
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
            ConflictException cfe => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = cfe.ConflictReason
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