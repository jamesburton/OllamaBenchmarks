using System.Net.Http.Json;
        using Microsoft.Extensions.DependencyInjection;
        using System.Text.Json.Serialization;

[System.Text.Json.Serialization.JsonPropertyName("login")]
        public string Login { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("public_repos")]
        public int PublicRepos { get; init; }

[System.Text.Json.Serialization.JsonPropertyName("login")]
        public record GitHubUser(
            string Login,
            [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
            [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
        );

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
            private readonly HttpClient httpClient;

            public GitHubClient(HttpClient httpClient)
            {
                this.httpClient = httpClient;
            }

            public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
            {
                return httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
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

using System.Net.Http.Json;
    using Microsoft.Extensions.DependencyInjection;
    using System.Text.Json.Serialization;

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
        private readonly HttpClient httpClient;

        public GitHubClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
        {
            return httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
        }
    }

    public static class GitHubClientExtensions
    {
        public static IServiceCollection AddGitHubClient(this IServiceCollection services)
        {
            return services.AddHttpClient<IGitHubClient, GitHubClient>();
        }
    }

using System.Net.Http.Json;
    using Microsoft.Extensions.DependencyInjection;
    using System.Text.Json.Serialization;

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
        private readonly HttpClient httpClient;

        public GitHubClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
        {
            return httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
        }
    }

    public static class GitHubClientExtensions
    {
        public static IServiceCollection AddGitHubClient(this IServiceCollection services)
        {
            return services.AddHttpClient<IGitHubClient, GitHubClient>();
        }
    }

using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;