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

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex) => 
        ex switch
        {
            NotFoundException notFound when notFound is NotFoundException => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = notFound.ResourceName
            },
            ConflictException conflict when conflict is ConflictException => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = conflict.ConflictReason
            },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = ex.Message
            }
        };
}

<hr>
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

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex) => 
        ex switch
        {
            NotFoundException notFound when notFound is NotFoundException => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = notFound.ResourceName
            },
            ConflictException conflict when conflict is ConflictException => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = conflict.ConflictReason
            },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = ex.Message
            }
        };
}

<hr>
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

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex) => 
        ex switch
        {
            NotFoundException notFound when notFound is NotFoundException => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = notFound.ResourceName
            },
            ConflictException conflict when conflict is ConflictException => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = conflict.ConflictReason
            },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = ex.Message
            }
        };
}

<hr>
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

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex) => 
        ex switch
        {
            NotFoundException notFound when notFound is NotFoundException => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = notFound.ResourceName
            },
            ConflictException conflict when conflict is ConflictException => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = conflict.ConflictReason
            },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = ex.Message
            }
        };
}

<hr>
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

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex) => 
        ex switch
        {
            NotFoundException notFound when notFound is NotFoundException => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = notFound.ResourceName
            },
            ConflictException conflict when conflict is ConflictException => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = conflict.ConflictReason
            },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = ex.Message
            }
        };
}

<hr>
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

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex) => 
        ex switch
        {
            NotFoundException notFound when notFound is NotFoundException => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = notFound.ResourceName
            },
            ConflictException conflict when conflict is ConflictException => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = conflict.ConflictReason
            },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = ex.Message
            }
        };
}

<hr>
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

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex) => 
        ex switch
        {
            NotFoundException notFound when notFound is NotFoundException => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = notFound.ResourceName
            },
            ConflictException conflict when conflict is ConflictException => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = conflict.ConflictReason
            },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = ex.Message
            }
        };
}

<hr>
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

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex) => 
        ex switch
        {
            NotFoundException notFound when notFound is NotFoundException => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = notFound.ResourceName
            },
            ConflictException conflict when conflict is ConflictException => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = conflict.ConflictReason
            },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = ex.Message
            }
        };
}

<hr>
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

static class ApiExceptionMapper
{
    public static ProblemDetails MapException(Exception ex) => 
        ex switch
        {
            NotFoundException notFound when notFound is NotFoundException => new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = notFound.ResourceName
            },
            ConflictException conflict when conflict is ConflictException => new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = conflict.ConflictReason
            },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = ex.Message
            }
        };
}