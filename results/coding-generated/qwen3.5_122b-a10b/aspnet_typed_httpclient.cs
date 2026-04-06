using System.Net.Http.Json;
        using Microsoft.Extensions.DependencyInjection;
        using System.Text.Json.Serialization; // Needed for JsonPropertyName
        using System.Threading; // Needed for CancellationToken
        using System.Threading.Tasks; // Needed for Task

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

                public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
                {
                    return _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
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
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

public record GitHubUser(
    [JsonPropertyName("login")] string Login,
    [JsonPropertyName("name")] string Name,
    [JsonPropertyName("public_repos")] int PublicRepos);

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

    public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
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

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

public record GitHubUser(
    [JsonPropertyName("login")] string Login,
    [JsonPropertyName("name")] string Name,
    [JsonPropertyName("public_repos")] int PublicRepos);

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

    public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
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

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

public record GitHubUser(
    [JsonPropertyName("login")] string Login,
    [JsonPropertyName("name")] string Name,
    [JsonPropertyName("public_repos")] int PublicRepos);

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

    public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
    {
        return _httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
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