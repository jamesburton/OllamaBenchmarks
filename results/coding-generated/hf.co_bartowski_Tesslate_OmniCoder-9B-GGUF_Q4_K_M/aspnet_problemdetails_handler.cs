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
    provided by the user.
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

var scoredItems = items.Select(item => new { Item = item, Score = CalculateScore(item) });

var scoredItems = items.Select(item => new { Item = item, Score = CalculateScore(item) });


c
var scoredItems = items.Select(item => new { Item = item, Score = CalculateScore(item) });

c
var scoredItems = code snippet

c
var scoredItems = items.Select(item => new { Item = item, Score = CalculateScore(item) });

c
var scoredItems = items.Select(yield return item => new { Item = item, Score = CalculateScore(item) });

c

c
var scoredItems = items.Select(item => new { Item =

c

c
var scoredItems = items.Select(item => new { Item = item, Score = CalculateScore(item) });

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c























































c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c

c


























c

c

c

c

c

c

c

c

c

c

c

c

c

c

c