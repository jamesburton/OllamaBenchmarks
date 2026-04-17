using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace WeatherApi
{
    // 1. Record
    public record WeatherForecast(string Location, double TemperatureC, string Condition, DateOnly Date);

    // 2. Interface
    public interface IWeatherClient
    {
        Task<WeatherForecast?> GetCurrentAsync(string location, CancellationToken ct);
        Task<IReadOnlyList<WeatherForecast>> GetForecastAsync(string location, int days, CancellationToken ct);
    }

    // 3. Typed HttpClient Implementation
    public class WeatherApiClient : IWeatherClient
    {
        private readonly HttpClient _httpClient;

        public WeatherApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Requirement: httpClient.BaseAddress must be set.
            // Note: In standard usage, BaseAddress is typically set via the HttpClientFactory 
            // configuration (as done in WeatherApiExtensions). 
            // If it is null here, we set it to avoid null reference exceptions in this slice, 
            // though the extension method handles the primary configuration.
            _httpClient.BaseAddress ??= new Uri("http://localhost");
        }

        public async Task<WeatherForecast?> GetCurrentAsync(string location, CancellationToken ct)
        {
            // GET /weather/current?location={location}
            var response = await _httpClient.GetAsync($"/weather/current?location={WebUtility.UrlEncode(location)}", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                // Throws HttpRequestException on other errors
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<WeatherForecast>(cancellationToken: ct);
        }

        public async Task<IReadOnlyList<WeatherForecast>> GetForecastAsync(string location, int days, CancellationToken ct)
        {
            // GET /weather/forecast?location={location}&days={days}
            var response = await _httpClient.GetAsync($"/weather/forecast?location={WebUtility.UrlEncode(location)}&days={days}", ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            var forecasts = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>(cancellationToken: ct);
            return forecasts ?? new List<WeatherForecast>();
        }
    }

    // 4. Retry Delegating Handler
    public class RetryDelegatingHandler : DelegatingHandler
    {
        private const int MaxRetries = 3;
        private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(100);

        public RetryDelegatingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }
        public RetryDelegatingHandler() { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage? response = null;
            Exception? lastException = null;

            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    // Clone the request for retries (HttpRequestMessage cannot be reused directly)
                    var requestClone = await CloneRequestAsync(request, cancellationToken);

                    response = await base.SendAsync(requestClone, cancellationToken);

                    // Retry on transient errors (5xx)
                    if (response.StatusCode >= HttpStatusCode.InternalServerError || 
                        response.StatusCode == HttpStatusCode.RequestTimeout)
                    {
                        lastException = new HttpRequestException($"Server error: {response.StatusCode}");
                        await Task.Delay(_delay, cancellationToken);
                        continue;
                    }

                    return response;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    await Task.Delay(_delay, cancellationToken);
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    // Timeout
                    lastException = new HttpRequestException("Request timed out", ex);
                    await Task.Delay(_delay, cancellationToken);
                }
            }

            throw lastException ?? new HttpRequestException("Retry policy failed.");
        }

        private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            if (request.Content != null)
            {
                clone.Content = await CloneContentAsync(request.Content, ct);
            }

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        private async Task<HttpContent> CloneContentAsync(HttpContent content, CancellationToken ct)
        {
            var stream = await content.ReadAsStreamAsync(ct);
            var memoryStream = new System.IO.MemoryStream();
            await stream.CopyToAsync(memoryStream, ct);
            memoryStream.Position = 0;

            var clone = new StreamContent(memoryStream);
            foreach (var header in content.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return clone;
        }
    }

    // 5. Extensions
    public static class WeatherApiExtensions
    {
        public static void AddWeatherClient(this IServiceCollection services, string baseUrl)
        {
            services.AddHttpClient<IWeatherClient, WeatherApiClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            })
            .AddHttpMessageHandler(() => new RetryDelegatingHandler());
        }
    }
}