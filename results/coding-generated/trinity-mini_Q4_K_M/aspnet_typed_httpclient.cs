using System;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

public record GitHubUser(string Login, string Name, int PublicRepos)
{
    [System.Text.Json.Serialization.JsonPropertyName("login")]
    public string Login { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("public_repos")]
    public int PublicRepos { get; init; }
}

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

using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

public record GitHubUser(string Login, string Name, int PublicRepos)
{
    [System.Text.Json.Serialization.JsonPropertyName("login")]
    public string Login { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("public_repos")]
    public int PublicRepos { get; init; }
}

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