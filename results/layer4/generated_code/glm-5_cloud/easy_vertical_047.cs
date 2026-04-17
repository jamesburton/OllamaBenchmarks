using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public record MenuItem(string Id, string Name, decimal Price, bool IsAvailable);

public interface IMenuRepository
{
    Task AddAsync(MenuItem item);
    Task<List<MenuItem>> GetAvailableAsync();
    Task<MenuItem?> GetByIdAsync(string id);
    Task UpdateAsync(MenuItem item);
}

public class MenuService
{
    private readonly IMenuRepository _repository;

    public MenuService(IMenuRepository repository)
    {
        _repository = repository;
    }

    public async Task ToggleAvailabilityAsync(string itemId)
    {
        var item = await _repository.GetByIdAsync(itemId);

        if (item is null)
        {
            throw new InvalidOperationException($"Menu item with ID '{itemId}' not found.");
        }

        var updatedItem = item with { IsAvailable = !item.IsAvailable };

        await _repository.UpdateAsync(updatedItem);
    }
}