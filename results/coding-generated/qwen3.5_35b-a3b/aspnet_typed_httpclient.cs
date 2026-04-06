using System.Net.Http.Json;
        using Microsoft.Extensions.DependencyInjection;
        using System.Text.Json.Serialization;
        using System.Threading;
        using System.Threading.Tasks;

public record GitHubUser(
                [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
                [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
                [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
            );

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
                    return services.AddHttpClient<IGitHubClient, GitHubClient>();
                }
            }

public record GitHubUser(
            [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
            [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
            [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
        );

public record GitHubUser(
            [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
            [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
            [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
        );

services.AddHttpClient<IGitHubClient, GitHubClient>();
        return services;

using System.Net.Http.Json;
    using Microsoft.Extensions.DependencyInjection;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    public record GitHubUser(
        [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
        [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
        [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
    );

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