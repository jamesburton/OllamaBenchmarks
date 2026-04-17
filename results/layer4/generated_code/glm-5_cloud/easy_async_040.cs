using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncStringBuilder
{
    public static async Task<string> BuildAsync(IEnumerable<Func<Task<string>>> parts)
    {
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var part in parts)
        {
            string result = await part().ConfigureAwait(false);
            stringBuilder.Append(result);
        }

        return stringBuilder.ToString();
    }
}