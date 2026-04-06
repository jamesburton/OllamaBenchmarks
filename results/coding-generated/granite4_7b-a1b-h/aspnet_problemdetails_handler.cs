using System;
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

public class NotFoundException : Exception
{
    public string ResourceName { get; }

    public NotFoundException(string resourceName)
    {
        ResourceName = resourceName;
    }
}

public class ConflictException : Exception
{
    public string ConflictReason { get; }

    public ConflictException(string conflictReason)
    {
        ConflictReason = conflictReason;
    }
}

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex)
    {
        switch (ex)
        {
            case NotFoundException notFound:
                return new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Not Found",
                    Detail = notFound.ResourceName
                };
            case ConflictException conflict:
                return new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Conflict",
                    Detail = conflict.ConflictReason
                };
            default:
                return new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = ex.Message
                };
        }
    }
}