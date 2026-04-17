using System;
using System.Collections.Generic;
using System.Linq;

public record TodoItem(int Id, string Title, bool IsCompleted);

public class TodoService
{
    private readonly List<TodoItem> _items = new();

    public void Add(TodoItem item)
    {
        _items.Add(item);
    }

    public void Complete(int id)
    {
        var index = _items.FindIndex(x => x.Id == id);
        if (index != -1)
        {
            _items[index] = _items[index] with { IsCompleted = true };
        }
    }

    public List<TodoItem> GetPending()
    {
        return _items.Where(x => !x.IsCompleted).ToList();
    }

    public List<TodoItem> GetAll()
    {
        return new List<TodoItem>(_items);
    }
}