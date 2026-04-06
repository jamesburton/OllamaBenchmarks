using System;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubClient
{
    public record GitHubUser(string Login, string Name, int PublicRepos)
    {
        [JsonPropertyName("login")]
        public string LoginStr { get; } = Login;

        [JsonPropertyName("name")]
        public string NameStr { get; } = Name;

        [JsonPropertyName("public_repos")]
        public int PublicReposStr { get; } = PublicRepos;
    }

    public interface IGitHubClient
    {
        Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default);
    }

    public class GitHubClient
    {
        private readonly HttpClient _httpClient;

        public GitHubClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<GitHubUser?>($"/users/{username}", ct);
                return result;
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately, e.g., log them
                Console.Error.WriteLine($"Error getting user: {ex}");
                return null;
            }
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
}