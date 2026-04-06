using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

record GitHubUser(
    [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
    [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
    [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
);

interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

class GitHubClient : IGitHubClient
{
    private readonly HttpClient httpClient;

    public GitHubClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
    }
}

static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        return services.AddHttpClient<IGitHubClient, GitHubClient>();
    }
}