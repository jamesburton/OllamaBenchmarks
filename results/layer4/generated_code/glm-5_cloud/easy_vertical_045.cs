using System;
using System.Collections.Generic;
using System.Linq;

public record Setting(string Key, string Value, string Category);

public class SettingsStore
{
    private readonly Dictionary<string, Setting> _settings = new();

    public void Set(Setting setting)
    {
        if (setting == null)
        {
            throw new ArgumentNullException(nameof(setting));
        }
        _settings[setting.Key] = setting;
    }

    public string? Get(string key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _settings.TryGetValue(key, out var setting) ? setting.Value : null;
    }

    public List<Setting> GetByCategory(string category)
    {
        if (category == null)
        {
            throw new ArgumentNullException(nameof(category));
        }

        return _settings.Values
            .Where(s => s.Category == category)
            .ToList();
    }

    public bool Delete(string key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _settings.Remove(key);
    }
}