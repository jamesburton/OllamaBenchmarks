using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

// 1. GitHubUser Record
public record GitHubUser(
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("public_repos")] int PublicRepos
);

// 2. IGitHubClient Interface
public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

// 3. GitHubClient Implementation
public class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;

    public GitHubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        var url = $"/users/{username}";
        // Use GetFromJsonAsync for deserialization
        var response = await _httpClient.GetAsync(url, ct);

        if (response.IsSuccessStatusCode)
        {
            // Note: GetFromJsonAsync handles the reading and deserialization
            var user = await response.Content.ReadFromJsonAsync<GitHubUser>(ct);
            return user;
        }

        return null;
    }
}

// 4. Static Extension Class
public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        services.AddHttpClient<IGitHubClient, GitHubClient>();
        return services;
    }
}