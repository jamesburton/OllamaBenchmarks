using System;

public class ImmutableStack
{
    private static readonly ImmutableStack empty = new ImmutableStack();
    public static ImmutableStack Empty => empty;

    private readonly T head;
    private readonly ImmutableStack tail;
    private readonly int count;

    public bool IsEmpty => count == 0;
    public int Count => count;

    private ImmutableStack()
    {
        count = 0;
    }

    private ImmutableStack(T head, ImmutableStack tail)
    {
        this.head = head;
        this.tail = tail;
        count = tail.count + 1;
    }

    public ImmutableStack Push(T item)
    {
        return new ImmutableStack(item, this);
    }

    public (T Top, ImmutableStack Rest) Pop()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Stack is empty.");

        return (head, tail);
    }
}