using System;
using System.Collections.Generic;
using System.Linq;

public record Subscription(string UserId, string Plan, DateTime RenewsAt, bool IsActive);

public class SubscriptionManager
{
    private readonly List<Subscription> _subscriptions = new();

    public void Add(Subscription subscription)
    {
        _subscriptions.Add(subscription);
    }

    public bool IsActive(string userId)
    {
        return _subscriptions.Any(s => s.UserId == userId && s.IsActive);
    }

    public List<Subscription> GetRenewingSoon(DateTime now, int withinDays)
    {
        return _subscriptions
            .Where(s => s.IsActive)
            .Where(s => s.RenewsAt > now && s.RenewsAt <= now.AddDays(withinDays))
            .ToList();
    }
}