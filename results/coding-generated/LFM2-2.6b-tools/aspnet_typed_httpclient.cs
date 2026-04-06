using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

public record GitHubUser(string Login, string Name, int PublicRepos) with JsonPropertyName("login"), JsonPropertyName("name"), JsonPropertyName("public_repos");

public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

public class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
        return response;
    }
}

public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        services.AddHttpClient<IGitHubClient, GitHubClient>();
        return services;
    }
}