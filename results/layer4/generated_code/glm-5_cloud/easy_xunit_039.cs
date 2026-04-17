using System;
using System.Collections.Generic;
using System.Linq;

public static class CollectionUtils
{
    public static Dictionary<TKey, List<TValue>> GroupBy<TKey, TValue>(IEnumerable<TValue> items, Func<TValue, TKey> keySelector)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }
        if (keySelector == null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var result = new Dictionary<TKey, List<TValue>>();

        foreach (var item in items)
        {
            var key = keySelector(item);
            if (!result.ContainsKey(key))
            {
                result[key] = new List<TValue>();
            }
            result[key].Add(item);
        }

        return result;
    }

    public static List<T> Flatten<T>(IEnumerable<IEnumerable<T>> nested)
    {
        if (nested == null)
        {
            throw new ArgumentNullException(nameof(nested));
        }

        var result = new List<T>();

        foreach (var innerCollection in nested)
        {
            if (innerCollection != null)
            {
                result.AddRange(innerCollection);
            }
        }

        return result;
    }
}