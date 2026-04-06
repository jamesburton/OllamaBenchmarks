using Microsoft.AspNetCore.Mvc;
using System;

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
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ProblemDetails(Exception ex)
    {
        switch (ex)
        {
            case typeof(NotFoundException) when ex is NotFoundException =>
                return new Microsoft.AspNetCore.Mvc.ProblemDetails
                { Title = "Not Found", Status = 404, Detail = ex.ResourceName };
            case typeof(ConflictException) when ex is ConflictException =>
                return new Microsoft.AspNetCore.Mvc.ProblemDetails
                { Title = "Conflict", Status = 409, Detail = ex.ConflictReason };
            default =>
                return new Microsoft.AspNetCore.Mvc.ProblemDetails
                { Title = "Internal Server Error", Status = 500, Detail = ex.Message };
        }
    }
}