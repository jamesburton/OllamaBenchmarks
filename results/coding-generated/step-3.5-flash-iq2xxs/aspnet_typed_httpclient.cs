public static IServiceCollection AddGitHubClient(this IServiceCollection services)
{
    services.AddHttpClient<IGitHubClient, GitHubClient>();
    return services;
}

public static IServiceCollection AddHttpClient<TClient, TClientImplementation>(this IServiceCollection serviceCollection) where TClient : class, TClientImplementation : class, TClient

using System;
using System.Net.Http.Json; // for GetFromJsonAsync, etc.
using System.Text.Json.Serialization; // for JsonPropertyName
using Microsoft.Extensions.DependencyInjection; // for IServiceCollection, etc.

public async Task<GitHubUser?> GetUserAsync(string username, CancellationToken ct = default)
{
    return await httpClient.GetFromJsonAsync<GitHubUser>($"/users/{username}", ct);
}