using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

public record GitHubUser(
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("public_repos")] int PublicRepos
);

public interface IGitHubClient
{
    Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
}

public class GitHubClient : IGitHubClient
{
    private readonly HttpClient httpClient;

    public GitHubClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
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