# ASP.NET Core 10 — Quick Reference

## Project File (no PackageReference for framework)

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

## Minimal API with Built-in Validation

```csharp
// [Required], [Range] etc. auto-validated in .NET 10
app.MapPost("/orders", ([Required] string name, [Range(1,100)] int qty) =>
    Results.Ok(new { name, qty }));
```

## Result Helpers

```csharp
Results.Ok(payload)           // 200
Results.Created("/path", obj) // 201
Results.NoContent()           // 204
Results.NotFound()            // 404
Results.BadRequest("msg")     // 400
Results.Problem(detail: "err", statusCode: 500)  // ProblemDetails RFC 9457
Results.ValidationProblem(errors)
```

## ProblemDetails Standard Error

```csharp
app.UseExceptionHandler(err => err.Run(async ctx =>
{
    ctx.Response.ContentType = "application/problem+json";
    await ctx.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Title = "Server Error",
        Status = 500,
        Detail = "Unexpected error occurred"
    });
}));
```

## DI Lifetimes

```csharp
services.AddScoped<IOrderService, OrderService>();    // per HTTP request
services.AddSingleton<ICache, MemoryCache>();         // app lifetime
services.AddTransient<IEmailSender, EmailSender>();  // new each injection
```

## Options Pattern

```csharp
// appsettings.json: { "Smtp": { "Host": "...", "Port": 587 } }
services.Configure<SmtpOptions>(config.GetSection("Smtp"));

// Inject:
public class EmailSender(IOptions<SmtpOptions> opts) { }
```

## Typed HttpClient

```csharp
services.AddHttpClient<IGitHubClient, GitHubClient>(c =>
    c.BaseAddress = new Uri("https://api.github.com/"));
```

**Source:** https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0
