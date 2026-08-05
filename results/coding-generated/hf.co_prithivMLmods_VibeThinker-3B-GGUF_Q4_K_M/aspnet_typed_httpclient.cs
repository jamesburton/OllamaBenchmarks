using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

record GitHubUser(
    [Text.Json.Serialize] string Login,
    [Text.Json.Serialize] string Name,
    [Text.Json.Serialize] int PublicRepos
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

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<GitHubUser>($/"/users/{username}", ct);
    }
}

public static class GitHubClientExtensions
{
    public static void AddGitHubClient(this IServiceCollection services)
    {
        services.AddHttpClient<IGitHubClient, GitHubClient>();
    }
}