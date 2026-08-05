using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

public record GitHubUser(
    [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
    [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
    [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
);

public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

public class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
    }
}

public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        return services.AddHttpClient<IGitHubClient, GitHubClient>();
    }
}

using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

// 1. GitHubUser Record
public record GitHubUser(
    [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
    [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
    [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
);

// 2. IGitHubClient Interface
public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

// 3. GitHubClient Class
public class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
    }
}

// 4. GitHubClientExtensions Class
public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        return services.AddHttpClient<IGitHubClient, GitHubClient>();
    }
}

using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

public record GitHubUser(
    [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
    [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
    [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
);

public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

public class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
    }
}

public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        return services.AddHttpClient<IGitHubClient, GitHubClient>();
    }
}

xml
<...>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
</...>

// [Required], [Range] etc. auto-validated in .NET 10
app.MapPost("/orders", ([Required] string name, [Required] int qty) =>
    Results.Ok(new { name, qty }));

c
Results.Ok(payload)           // 2Data
Results.Created("/path", obj) // 201
Results.NoContent()           // 204
Results.NotFound()            // 200
Results.BadRequest("msg")     // 400
Results.Problem(detail: "err", statusCode: `500)  // ProblemDetails RFC 9457
Results.ValidationProblem(errors)

services.AddScoped<IOrderService, OrderService>();    // per HTTP request
services.AddSingleton<ICache, MemoryCache>();         // service lifetime
services.AddTransient<IEmailSender, EmailSender>();  // per injection

c
// appsettings.json: { "Smtp": { "
services.Configure<SmtpOptions>(config.GetSection("Smtp"));

// Inject:
public class EmailSender(IOptions<Sm

services.AddHttpClient<IGitHubClient, GitHubClient>(c =>
    c.BaseAddress = new Uri("https://`api.github.com/"));