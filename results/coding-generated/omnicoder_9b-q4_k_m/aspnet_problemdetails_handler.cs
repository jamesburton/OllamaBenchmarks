public class NotFoundException : Exception
        {
            public string ResourceName { get; }

            public NotFoundException(string resourceName)
            {
                ResourceName = resourceName;
                base($"Resource not found: {resourceName}");
            }
        }

public NotFoundException(string resourceName) : base($"Resource not found: {resourceName}")
        {
            ResourceName = resourceName;
        }

public NotFoundException(string resourceName)
        {
            ResourceName = resourceName;
            base($"Resource not found: {resourceName}"); // Invalid syntax
        }

public NotFoundException(string resourceName) : base($"Resource not found: {resourceName}"))
        {
            ResourceName = resourceName;
        }

public NotFoundException(string resourceName) : base($"Resource not found: {resourceName}")
        {
            ResourceName = resourceName;
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
                    ConflictException ce => new ProblemDetails
                    {
                        Status = 409,
                        Title = "Conflict",
                        Detail = ce.ConflictReason
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

await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Server Error",
            Status = 500,
            Detail = "Unexpected error occurred"
        });

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
            public static Problem
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
                        ConflictException ce => new ProblemDetails
                        {
                            Status = 409,
                            Title = "Conflict",
                            Detail = ce.ConflictReason
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

public NotFoundException(string resourceName) : base($"Resource not found: {resourceName}")
        {
            ResourceName = resourceName;
        }

return ex switch
        {
            NotFoundException nfe => ...,
            ConflictException ce => ...,
            _ => ...
        };

is a property (int).). `Title` is string. `Detail` is string.
    *   Wait, `ProblemDetails` in `Microsoft.AspNetCore.Mvc` has `StatusCode` property usually, but sometimes `Status`. Let's check standard ASP.NET Core `ProblemDetails`.
    *   Standard `ProblemDetails` (System.Text.Json / Microsoft.AspNetCore.Mvc):
        *   `StatusCode` (int)
        *   `Title` (string)
        *   `Detail` (string)
        *   `Instance` (string)
        *   `Type` (string)
    *   However, the provided reference says:

Wait, looking closely at the provided reference text:

The reference explicitly uses `Status`. Even if the actual SDK uses `StatusCode`, I must follow the reference provided in the prompt context or standard conventions?
        The prompt says: "Source: https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0". This implies a hypothetical or future version where `Status` might be the property name, or the reference text provided in the prompt is the source of truth for the API surface.
        However, standard `ProblemDetails` in .NET 8/9 uses `StatusCode`.
        But the prompt instruction says: "Generate a single C# file containing...". It implies I should write code that compiles based on the provided reference context.
        Wait, `Results.Pro` section in the prompt:

And the Exception Handler section:

Okay, the prompt's provided reference explicitly uses `Status = 500`. I should probably use `Status` to match the provided reference's API surface, even if standard .NET uses `StatusCode`.
        Wait, actually, looking at the `ProblemDetails` class in `Microsoft.AspNetCore.Mvc` (standard), it has `StatusCode`.
        However, if I write `Status = 500`, it might not compile on standard .NET.
        But the instruction says "Given the following API reference...".
        If I use `StatusCode`, it might deviate from the reference provided.
        However, `ProblemDetails` is a standard type.
        Let's look at the instruction again: "ProblemDetails is in the `Microsoft.AspNetCore.Mvc` namespace (available via `FrameworkReference` to `Microsoft.AspNetCore.App`)."
        If I use `Status`, and the actual type has `StatusCode`, it won't compile.
        But the prompt's reference snippet shows `Status = 500`.
        To be safe and compliant with the prompt's provided "API"" (which is the reference), I should follow the reference's property name if possible, but `ProblemDetails` is a well-known type.
        Actually, in .NET 8+, `ProblemDetails` has `StatusCode`.
        If I write `Status`, it will be a compile error on standard .NET.
        However, the prompt asks me to generate code based on the reference.
        Let's check the reference snippet again: