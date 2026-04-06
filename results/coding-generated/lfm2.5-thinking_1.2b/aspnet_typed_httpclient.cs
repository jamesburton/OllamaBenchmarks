using System;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

class GitHubUser
{
    public string Login { get; set; }
    public string Name { get; set; }
    public int PublicRepos { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("Login")]
    public string JsonPropertyName { }
    [System.Text.Json.Serialization.JsonPropertyName("Name")]
    public string NameJson { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("PublicRepos")]
    public string PublicReposJson { get; set; }
}

public class GitHubClient
{
    private readonly HttpClient _httpClient;
    public GitHubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GitHubUser> GetUserAsync(string username, CancellationToken ct = default)
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