using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

record GitHubUser(string Login, string Name, int PublicRepos) {
    [System.Text.Json.Serialization.JsonPropertyName("login")]
    public string Login { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("public_repos")]
    public int PublicRepos { get; init; }
}

interface IGitHubClient {
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

class GitHubClient : IGitHubClient {
    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default) {
        return _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
    }
}

public static class GitHubClientExtensions {
    public static IServiceCollection AddGitHubClient(this IServiceCollection services) {
        return services.AddHttpClient<IGitHubClient, GitHubClient>();
    }
}