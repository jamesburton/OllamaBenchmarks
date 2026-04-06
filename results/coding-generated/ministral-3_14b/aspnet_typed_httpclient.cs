using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

public record GitHubUser(
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("public_repos")] int PublicRepos);

public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

public class GitHubClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
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