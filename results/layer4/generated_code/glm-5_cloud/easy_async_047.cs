using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class Dispatcher
{
    public static async Task DispatchAsync(IEnumerable<Func<Task>> handlers)
    {
        if (handlers == null)
        {
            throw new ArgumentNullException(nameof(handlers));
        }

        foreach (var handler in handlers)
        {
            if (handler != null)
            {
                await handler().ConfigureAwait(false);
            }
        }
    }
}