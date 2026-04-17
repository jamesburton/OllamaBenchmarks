using System;
using System.Collections.Generic;

public class EventBus
{
    private readonly Dictionary<string, List<Action>> _handlers = new Dictionary<string, List<Action>>();

    public void Subscribe(string eventType, Action handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Action>();
        }

        _handlers[eventType].Add(handler);
    }

    public void Publish(string eventType)
    {
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            // Create a copy of the list to allow handlers to unsubscribe during invocation safely
            var handlersCopy = handlers.ToArray();
            foreach (var handler in handlersCopy)
            {
                handler.Invoke();
            }
        }
    }

    public int HandlerCount(string eventType)
    {
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            return handlers.Count;
        }
        return 0;
    }
}