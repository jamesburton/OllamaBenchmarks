using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

[System.Text.Json.Serialization.JsonPropertyName("login")]
public record GitHubUser(string Login, string Name, int PublicRepos);

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

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
    }
}

public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        return services.AddHttpClient<IGitHubClient, GitHubClient>();
    }
}