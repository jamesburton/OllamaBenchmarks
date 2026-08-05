using System;
using Microsoft.AspNetCore.Mvc;

public class NotFoundException : Exception {
    public string ResourceName { get; set; }

    public NotFoundException(string resourceName) : base($"Resource not found: {resourceName}") {
        this.ResourceName = resourceName;
    }
}

public class ConflictException : Exception {
    public string ConflictReason { get; set; }

    public ConflictException(string conflictReason) : base($"Conflict: {conflictReason}") {
        this.ConflictReason = conflictReason;
    }
}

public static class ApiExceptionMapper {
    public static Microsoft.AspNetCore.Mvc.ProblemDetails MapException(Exception ex) {
        switch (ex) {
            case NotFoundException notFound:
                return new ProblemDetails
                {
                    Title = "Not Found",
                    Status = 404,
                    Detail = notFound.ResourceName
                };
            case ConflictException conflict:
                return new ProblemDetails
                {
                    Title = "Conflict",
                    Status = 409,
                    Detail = conflict.ConflictReason
                };
            default:
                return new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = 500,
                    Detail = ex.Message
                };
        }
    }
}