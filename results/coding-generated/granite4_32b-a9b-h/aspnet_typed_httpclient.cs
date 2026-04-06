using System;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

record GitHubUser(string Login, string Name, int PublicRepos)
{
    [System.Text.Json.Serialization.JsonPropertyName("login")] public string Login { get; init; } = Login!;
    [System.Text.Json.Serialization.JsonPropertyName("name")]  public string Name   { get; init; } = Name!;
    [System.Text.Json.Serialization.JsonPropertyName("public_repos")] public int PublicRepos { get; init; } = PublicRepos;
}

interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

class GitHubClient : IGitHubClient
{
    private readonly HttpClient httpClient;

    public GitHubClient(HttpClient httpClient)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(username))
            return null;

        try
        {
            return await httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            // Handle or log the exception as needed
            return null;
        }
    }
}

public static class GitHubClientExtensions
{
    public static IServiceCollection AddGitHubClient(this IServiceCollection services)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        return services.AddHttpClient<IGitHubClient, GitHubClient>();
    }
}