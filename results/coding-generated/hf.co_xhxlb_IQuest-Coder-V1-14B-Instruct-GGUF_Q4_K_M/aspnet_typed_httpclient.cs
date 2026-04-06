using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

// 1. Positional record with JsonPropertyNames
public record GitHubUser(
    [property: System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
    [property: System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
    [property: System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos);

// 2. Interface
public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

// 3. Implementation
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

// 4. Extension method
public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        services.AddHttpClient<IGitHubClient, GitHubClient>();
        return services;
    }
}