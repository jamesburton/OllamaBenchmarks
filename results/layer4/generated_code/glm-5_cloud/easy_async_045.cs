using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncMapper
{
    public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(
        IEnumerable<TKey> keys,
        Func<TKey, Task<TValue>> valueFactory)
        where TKey : notnull
    {
        if (keys == null)
        {
            throw new ArgumentNullException(nameof(keys));
        }

        if (valueFactory == null)
        {
            throw new ArgumentNullException(nameof(valueFactory));
        }

        var result = new Dictionary<TKey, TValue>();

        foreach (var key in keys)
        {
            // Call the factory sequentially for each key
            var value = await valueFactory(key);
            result.Add(key, value);
        }

        return result;
    }
}