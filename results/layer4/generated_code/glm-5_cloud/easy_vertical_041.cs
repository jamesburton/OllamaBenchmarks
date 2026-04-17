using System;
using System.Collections.Generic;
using System.Linq;

public record ApiKey(string Key, string OwnerId, bool IsRevoked, DateTime CreatedAt);

public class ApiKeyManager
{
    private readonly List<ApiKey> _apiKeys = new List<ApiKey>();

    public ApiKey Generate(string ownerId)
    {
        var newKey = new ApiKey(
            Key: Guid.NewGuid().ToString(),
            OwnerId: ownerId,
            IsRevoked: false,
            CreatedAt: DateTime.UtcNow
        );

        _apiKeys.Add(newKey);
        return newKey;
    }

    public void Revoke(string key)
    {
        var index = _apiKeys.FindIndex(k => k.Key == key);
        if (index != -1)
        {
            // Use 'with' expression to create a new record with IsRevoked set to true
            _apiKeys[index] = _apiKeys[index] with { IsRevoked = true };
        }
    }

    public bool IsValid(string key)
    {
        var apiKey = _apiKeys.FirstOrDefault(k => k.Key == key);
        return apiKey != null && !apiKey.IsRevoked;
    }
}