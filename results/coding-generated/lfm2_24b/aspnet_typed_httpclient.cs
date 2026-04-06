using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System;

// GitHubUser record with JSON property names
[System.Text.Json.Serialization.SerializationAttribute(System.Text.Json.Serialization.JsonPropertyName("login"), System.Text.Json.Serialization.JsonPropertyName("name"), System.Text.Json.Serialization.JsonPropertyName("public_repos")]
public record GitHubUser(string Login, string Name, int PublicRepos);

// Interface definition
public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

// Implementation of IGitHubClient
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

// Extension method for IServiceCollection
public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        services.AddHttpClient<IGitHubClient, GitHubClient>();
        return services;
    }
}