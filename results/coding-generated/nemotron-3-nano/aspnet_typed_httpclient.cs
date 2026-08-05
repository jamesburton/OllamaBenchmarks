using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

public record GitHubUser(
    [JsonPropertyName("login")] string Login,
    [JsonPropertyName("name")] string Name,
    [JsonPropertyName("public_repos")] int PublicRepos) { }

public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

public class GitHubClient( HttpClient httpClient ) : IGitHubClient
{
    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default) =>
        await httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
}

public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services) =>
        services.AddHttpClient<IGitHubClient, GitHubClient>();
}