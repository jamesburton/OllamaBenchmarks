using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiKeyAuthentication;

/// <summary>
/// Options for API Key authentication.
/// </summary>
public class ApiKeyAuthOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = "X-Api-Key";
}

/// <summary>
/// Interface for validating API keys and retrieving the associated client ID.
/// </summary>
public interface IApiKeyStore
{
    /// <summary>
    /// Retrieves the client ID associated with the provided API key.
    /// </summary>
    /// <param name="apiKey">The API key to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The client ID if valid; otherwise, null.</returns>
    Task<string?> GetClientIdAsync(string apiKey, CancellationToken ct);
}

/// <summary>
/// Simple in-memory implementation of IApiKeyStore.
/// </summary>
public class InMemoryApiKeyStore : IApiKeyStore
{
    private readonly Dictionary<string, string> _apiKeys = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryApiKeyStore(IDictionary<string, string>? initialKeys = null)
    {
        if (initialKeys != null)
        {
            foreach (var kvp in initialKeys)
            {
                _apiKeys[kvp.Key] = kvp.Value;
            }
        }
    }

    public Task<string?> GetClientIdAsync(string apiKey, CancellationToken ct)
    {
        _apiKeys.TryGetValue(apiKey, out var clientId);
        return Task.FromResult(clientId);
    }
}

/// <summary>
/// Authentication handler for API Key validation.
/// </summary>
public class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthOptions>
{
    private readonly IApiKeyStore _apiKeyStore;

    public ApiKeyAuthHandler(
        IOptionsMonitor<ApiKeyAuthOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder,
        IApiKeyStore apiKeyStore)
        : base(options, logger, encoder)
    {
        _apiKeyStore = apiKeyStore;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if the header exists
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var headerValue) || string.IsNullOrEmpty(headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        var apiKey = headerValue.ToString();
        var clientId = await _apiKeyStore.GetClientIdAsync(apiKey, Context.RequestAborted);

        if (string.IsNullOrEmpty(clientId))
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        // Create claims
        var claims = new List<Claim>
        {
            new Claim("client-id", clientId)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";

        var errorResponse = new { error = "Invalid API key" };
        var json = JsonSerializer.Serialize(errorResponse);

        await Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension methods for setting up API Key authentication.
/// </summary>
public static class ApiKeyAuthExtensions
{
    public const string DefaultScheme = "ApiKey";

    /// <summary>
    /// Adds API Key authentication to the service collection.
    /// </summary>
    public static IServiceCollection AddApiKeyAuthentication(
        this IServiceCollection services,
        Action<ApiKeyAuthOptions>? configure = null)
    {
        // Register the handler
        services.AddAuthentication()
            .AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>(DefaultScheme, configure);

        // Register the store (default singleton)
        services.AddSingleton<IApiKeyStore, InMemoryApiKeyStore>();

        return services;
    }
}