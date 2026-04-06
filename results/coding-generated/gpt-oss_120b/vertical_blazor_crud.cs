using Microsoft.AspNetCore.Components;

public record TodoItem(int Id, string Title, bool IsCompleted);

public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

public class TodoService : ITodoService
{
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        var copy = _items.Select(i => i).ToList();
        return Task.FromResult(copy);
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
            _items.Remove(item);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            var toggled = item with { IsCompleted = !item.IsCompleted };
            var index = _items.IndexOf(item);
            _items[index] = toggled;
        }
        return Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            await LoadAsync();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadAsync();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadAsync();
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();

        var item = await service.AddAsync("Test");

        item.Title.Should().Be("Test");
        item.Id.Should().Be(1);
        item.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var item1 = await service.AddAsync("One");
        var item2 = await service.AddAsync("Two");

        await service.DeleteAsync(item1.Id);
        var all = await service.GetAllAsync();

        all.Count.Should().Be(1);
        all[0].Id.Should().Be(item2.Id);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Item");

        await service.ToggleAsync(item.Id);
        var after = (await service.GetAllAsync()).First(i => i.Id == item.Id);
        after.IsCompleted.Should().BeTrue();

        await service.ToggleAsync(item.Id);
        var after2 = (await service.GetAllAsync()).First(i => i.Id == item.Id);
        after2.IsCompleted.Should().BeFalse();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void OnInitialized_CallsGetAllAsync()
    {
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

        using var ctx = new Bunit.TestContext();
        Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<ITodoService>(ctx.Services, service);

        var cut = ctx.RenderComponent<TodoListBase>();

        service.Received().GetAllAsync();
    }
}