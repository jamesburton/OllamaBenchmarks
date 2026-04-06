using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

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