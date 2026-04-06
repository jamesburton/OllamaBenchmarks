[System.Text.Json.Serialization.JsonPropertyName("login")]
[System.Text.Json.Serialization.JsonPropertyName("name")]
[System.Text.Json.Serialization.JsonPropertyName("public_repos")]
record GitHubUser(string Login, string Name, int PublicRepos);

interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct) ?? null;
    }
}

public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        services.AddHttpClient<IGitHubClient, GitHubClient>(c =>
            c.BaseAddress = new Uri("https://api.github.com/"));
        return services;
    }
}