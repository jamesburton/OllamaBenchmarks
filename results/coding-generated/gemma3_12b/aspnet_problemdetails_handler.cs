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
        ProblemDetails problem = new ProblemDetails();

        ex switch
        {
            NotFoundException notFoundException =>
            {
                problem.Status = 404;
                problem.Title = "Not Found";
                problem.Detail = notFoundException.ResourceName;
            }
            ConflictException conflictException =>
            {
                problem.Status = 409;
                problem.Title = "Conflict";
                problem.Detail = conflictException.ConflictReason;
            }
            _ =>
            {
                problem.Status = 500;
                problem.Title = "Internal Server Error";
                problem.Detail = ex.Message;
            }
        }

        return problem;
    }
}