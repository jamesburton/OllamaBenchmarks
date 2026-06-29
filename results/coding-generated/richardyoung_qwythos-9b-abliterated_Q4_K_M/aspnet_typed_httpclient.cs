using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

record GitHubUser(string Login, string Name, int PublicRepos)
{
    [System.Text.Json.Serialization.JsonPropertyName("login")]
    public string Login { get; init; } = default!;

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [System.Text.Json.Serialization.JsonPropertyName("public_repos")]
    public int PublicRepos { get; init; } = default!;
}

interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient) => _httpClient = httpClient;

    public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
        => _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
}

static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
        => services.AddHttpClient<IGitHubClient, GitHubClient>();
}