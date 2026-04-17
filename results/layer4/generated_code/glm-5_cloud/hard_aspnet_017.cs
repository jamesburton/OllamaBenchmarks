using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WebhookSystem
{
    // --- Records ---

    public record WebhookSubscription(Guid Id, string EventType, string TargetUrl, bool IsActive);

    public record WebhookEvent(string EventType, object Payload, DateTime OccurredAt);

    public record DispatchResult(int TotalSubscribers, int SuccessCount, int FailureCount, IReadOnlyList<string> Errors);


    // --- Interfaces ---

    public interface IWebhookRegistry
    {
        Task<Guid> SubscribeAsync(string eventType, string targetUrl, CancellationToken ct);
        Task<bool> UnsubscribeAsync(Guid subscriptionId, CancellationToken ct);
        Task<IReadOnlyList<WebhookSubscription>> GetSubscriptionsAsync(string eventType, CancellationToken ct);
    }

    public interface IWebhookDispatcher
    {
        Task<DispatchResult> DispatchAsync(WebhookEvent evt, CancellationToken ct);
    }


    // --- Implementations ---

    public class InMemoryWebhookRegistry : IWebhookRegistry
    {
        private readonly ConcurrentDictionary<Guid, WebhookSubscription> _subscriptions = new();

        public Task<Guid> SubscribeAsync(string eventType, string targetUrl, CancellationToken ct)
        {
            var subscription = new WebhookSubscription(Guid.NewGuid(), eventType, targetUrl, true);
            _subscriptions[subscription.Id] = subscription;
            return Task.FromResult(subscription.Id);
        }

        public Task<bool> UnsubscribeAsync(Guid subscriptionId, CancellationToken ct)
        {
            if (_subscriptions.TryGetValue(subscriptionId, out var sub))
            {
                // Mark as inactive instead of removing to preserve history/integrity if needed,
                // or remove completely. Here we mark inactive.
                _subscriptions[subscriptionId] = sub with { IsActive = false };
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<IReadOnlyList<WebhookSubscription>> GetSubscriptionsAsync(string eventType, CancellationToken ct)
        {
            var list = _subscriptions.Values
                .Where(s => s.EventType == eventType && s.IsActive)
                .ToList();
            return Task.FromResult<IReadOnlyList<WebhookSubscription>>(list);
        }
    }

    public class WebhookDispatcher : IWebhookDispatcher
    {
        private readonly IWebhookRegistry _registry;
        private readonly IHttpClientFactory _httpClientFactory;

        public WebhookDispatcher(IWebhookRegistry registry, IHttpClientFactory httpClientFactory)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<DispatchResult> DispatchAsync(WebhookEvent evt, CancellationToken ct)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));

            var subscriptions = await _registry.GetSubscriptionsAsync(evt.EventType, ct);

            // If no subscribers, return empty result
            if (subscriptions.Count == 0)
            {
                return new DispatchResult(0, 0, 0, Array.Empty<string>());
            }

            var client = _httpClientFactory.CreateClient();
            var errors = new List<string>();
            int successCount = 0;
            int failureCount = 0;

            // Serialize payload once
            var jsonPayload = JsonSerializer.Serialize(evt.Payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Dispatch to all subscribers concurrently
            var dispatchTasks = subscriptions.Select(async sub =>
            {
                try
                {
                    // We use a generic try-catch block per request
                    var response = await client.PostAsync(sub.TargetUrl, content, ct);

                    if (response.IsSuccessStatusCode)
                    {
                        return (Success: true, Error: (string?)null);
                    }
                    else
                    {
                        return (Success: false, Error: $"Webhook to {sub.TargetUrl} failed with status code {response.StatusCode}.");
                    }
                }
                catch (Exception ex)
                {
                    return (Success: false, Error: $"Webhook to {sub.TargetUrl} failed with exception: {ex.Message}");
                }
            });

            var results = await Task.WhenAll(dispatchTasks);

            foreach (var result in results)
            {
                if (result.Success)
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                    if (result.Error != null)
                    {
                        errors.Add(result.Error);
                    }
                }
            }

            return new DispatchResult(subscriptions.Count, successCount, failureCount, errors);
        }
    }
}