public class NotFoundException : Exception
        {
            public string ResourceName { get; }
            public NotFoundException(string resourceName) : base($"Resource not’s not found: {resourceName}") {
                ResourceName = resourceName;
            }
        }

public static class ApiExceptionMapper {
            public static ProblemDetails MapException(Exception ex) {
                return ex switch {
                    NotFoundException nf => new ProblemDetails { Status = 404, Title = "Not Found", Detail = nf.ResourceName },
                    ConflictException cf => new ProblemDetails { Status = 409, Title = "Conflict", Detail = cf.ConflictReason },
                    _ => new ProblemDetails { Status = 500, Title = "Internal Server Error", Detail = ex.Message }
                };
            }
        }