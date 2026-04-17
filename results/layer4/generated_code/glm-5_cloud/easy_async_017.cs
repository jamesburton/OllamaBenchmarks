using System.Collections.Generic;
using System.Threading.Tasks;

public class AsyncRepository
{
    private readonly List<string> _items = new List<string>();

    public async Task AddAsync(string item)
    {
        await Task.Yield();
        _items.Add(item);
    }

    public async Task<List<string>> GetAllAsync()
    {
        await Task.Yield();
        return new List<string>(_items);
    }
}