using System.Net.Http.Json;
        using Microsoft.Extensions.DependencyInjection;

[System.Text.Json.Serialization.JsonPropertyName("login")]
        public record GitHubUser(string Login); // Wait, positional syntax means properties are defined in order.

public record GitHubUser(string Login, string Name, int PublicRepos);

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

public static IServiceCollection AddGitHubClient(this IServiceCollection services)
        {
            return services.AddHttpClient<IGitHubClient, GitHubClient>();
        }

public record GitHubUser(
            [System.Text.Json.Serialization.JsonPropertyName("login")] string Login,
            [System.Text.Json.Serialization.JsonPropertyName("name")] string Name,
            [System.Text.Json.Serialization.JsonPropertyName("public_repos")] int PublicRepos
        );

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