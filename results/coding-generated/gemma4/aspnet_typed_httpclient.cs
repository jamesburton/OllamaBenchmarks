using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

// 1. GitHubUser record
public record GitHubUser(
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("public_repos")] int PublicRepos
);

// 2. IGitHubClient interface
public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

// 3. GitHubClient implementation
public class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        // The API reference suggests using the typed client pattern, 
        // which typically involves calling GetFromJsonAsync on the base client.
        return await _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
    }
}

// 4. GitHubClientExtensions static class
public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        // This mimics the usage shown in the API reference:
        // services.AddHttpClient<IGitHubClient, GitHubClient>(c => c.BaseAddress = new Uri("https://api.github.com/"));
        services.AddHttpClient<IGitHubClient, GitHubClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com/");
        });
        return services;
    }
}