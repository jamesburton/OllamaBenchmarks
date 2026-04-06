using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

public record GitHubUser(
    [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
    [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
    [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos);

public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

public class GitHubClient(HttpClient httpClient) : IGitHubClient
{
    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
    }
}

public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        return services.AddHttpClient<IGitHubClient, GitHubClient>();
    }
}