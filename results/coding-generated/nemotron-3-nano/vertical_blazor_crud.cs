using Microsoft.AspNetCore.Components;

record TodoItem(int Id, string Title, bool IsCompleted);

interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

class TodoService : ITodoService
{
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_items.ToList());

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        _items.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var idx = _items.FindIndex(i => i.Id == id);
        if (idx >= 0)
        {
            var old = _items[idx];
            var updated = old with { IsCompleted = !old.IsCompleted };
            _items[idx] = updated;
            return Task.FromResult(updated);
        }
        return Task.FromResult<TodoItem>(default)!;
    }
}

class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

    public List<TodoItem> Todos { get; set; } = new();

    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = (await TodoService.GetAllAsync()).ToList();
    }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        Todos = (await TodoService.GetAllAsync()).ToList();
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos = (await TodoService.GetAllAsync()).ToList();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        Todos = (await TodoService.GetAllAsync()).ToList();
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test Title");
        item.Title.Should().Be("Test Title");
        item.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Item1");
        await service.AddAsync("Item2");
        var all = await service.GetAllAsync();
        all.Count.Should().Be(2);
        await service.DeleteAsync(1);
        var remaining = await service.GetAllAsync();
        remaining.Count.Should().Be(1);
        remaining.First().Title.Should().Be("Item2");
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Task");
        var before = item.IsCompleted;
        await service.ToggleAsync(item.Id);
        var afterItems = await service.GetAllAsync();
        var toggled = afterItems.First(i => i.Id == item.Id);
        toggled.IsCompleted.Should().Be(!before);
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void TodoListBase_Renders_And_Calls_GetAllAsync_OnInit()
    {
        var mockService = Substitute.For<ITodoService>();
        var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<TodoListBase>(params => params.Add(p => p.TodoService, mockService));
        mockService.Received(1).GetAllAsync();
    }
}