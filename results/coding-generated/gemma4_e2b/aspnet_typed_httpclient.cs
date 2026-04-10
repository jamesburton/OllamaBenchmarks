using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

// 1. GitHubUser record
public record GitHubUser(
    [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
    [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
    [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
);

// 2. IGitHubClient interface
public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

// 3. GitHubClient class
public class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        // The API reference implies using GetFromJsonAsync for deserialization
        return await _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
    }
}

// 4. Static extension class
public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        services.AddHttpClient<IGitHubClient, GitHubClient>(c =>
        {
            c.BaseAddress = new Uri("https://api.github.com/");
        });
        return services;
    }
}