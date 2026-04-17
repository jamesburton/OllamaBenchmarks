using System;
using System.Collections.Generic;

public class Queue<T>
{
    private readonly LinkedList<T> _items = new LinkedList<T>();

    public void Enqueue(T item)
    {
        _items.AddLast(item);
    }

    public T Dequeue()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        T value = _items.First.Value;
        _items.RemoveFirst();
        return value;
    }

    public T Peek()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        return _items.First.Value;
    }

    public int Count
    {
        get { return _items.Count; }
    }

    public bool IsEmpty
    {
        get { return _items.Count == 0; }
    }
}