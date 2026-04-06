using Microsoft.AspNetCore.Mvc;

namespace NotFoundExceptionAndApiExceptionMapper
{
    public class NotFoundException : Exception
    {
        public string ResourceName { get; }

        public NotFoundException(string resourceName)
            : base(resourceName)
        {
            ResourceName = resourceName;
        }
    }

    public class ConflictException : Exception
    {
        public string ConflictReason { get; }

        public ConflictException(string conflictReason)
            : base(conflictReason)
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
                NotFoundException => new ProblemDetails
                {
                    Status = 404,
                    Title = "Not Found",
                    Detail = ex.ResourceName
                },
                ConflictException => new ProblemDetails
                {
                    Status = 409,
                    Title = "Conflict",
                    Detail = ex.ConflictReason
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
}