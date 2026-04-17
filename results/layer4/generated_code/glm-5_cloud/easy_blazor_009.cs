using Microsoft.AspNetCore.Components;

public class TodoListComponent : ComponentBase
{
    public List<string> Items { get; private set; } = new();

    public void AddItem(string item)
    {
        if (!string.IsNullOrWhiteSpace(item))
        {
            Items.Add(item);
        }
    }

    public void RemoveItem(string item)
    {
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    public int Count => Items.Count;
}