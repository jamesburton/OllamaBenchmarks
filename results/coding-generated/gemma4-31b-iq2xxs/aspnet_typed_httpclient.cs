public record GitHubUser(
            [JsonPropertyName("login")] string Login,
            [JsonPropertyName("name")] string Name,
            [JsonPropertyName("public_repos")] int PublicRepos
        );

public interface IGitHubClient {
            Task<GitHubUser?> GetUserAsync(string username, CancellationingToken ct = default);
        }

public class GitHubClient : IGitHubClient {
            private readonly HttpClient _httpClient;
            public GitHubClient(HttpClient httpClient) {
                _httpClient = httpClient;
            }
            public async Task<GitHubUser?> GetUserAsync(string username, CancellationingToken ct = default) {
                return await _httpClient.GetFromJsonsAsync<GitHubUser>($"/users/{username}", ct);
            }
        s}

public static class GitHubClientExtensions {
            public static IServiceCollection AddGitHubClient(this IServiceCollection services) {
                services.AddHttpClient<IGitHubClient, GitHubClientH>(); // Wait, the prompt says AddHttpClient<IGitHubClient, GitHubClient>()
                return services;
1S}