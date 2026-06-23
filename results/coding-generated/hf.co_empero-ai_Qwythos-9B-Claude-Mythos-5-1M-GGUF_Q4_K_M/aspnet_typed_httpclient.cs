using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

record GitHubUser(
    [JsonPropertyName("login")] string login,
    [JsonPropertyName("name")] string name,
    [JsonPropertyName("public_repos")] int publicRepos
);

interface IGitHubClient {
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

class GitHubClient : IGitHubClient {
    private readonly HttpClient httpClient;
    public GitHubClient(HttpClient httpClient) => this.httpClient = httpClient;
    public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default) =>
        httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
}

static class GitHubClientExtensions {
    public static IServiceCollection AddGitHubClient(this IServiceCollection services) =>
        services.AddHttpClient<IGitHubClient, GitHubClient>();
}