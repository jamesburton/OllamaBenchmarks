using System;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

global record GitHubUser(string login, string name, int publicRepos)
{
    [JsonPropertyName("login")]
    public string Login => login;

    [JsonPropertyName("name")]
    public string Name => name;

    [JsonPropertyName("public_repos")]
    public int PublicRepos => publicRepos;
}

global interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

global class GitHubClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
    }
}

global static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        services.AddHttpClient<IGitHubClient, GitHubClient>();
        return services;
    }
}