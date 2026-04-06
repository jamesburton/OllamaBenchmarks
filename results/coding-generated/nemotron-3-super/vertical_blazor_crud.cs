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
    private List<TodoItem> _items = new();
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
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null) _items.Remove(item);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            var index = _items.IndexOf(item);
            var toggled = item with { IsCompleted = !item.IsCompleted };
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
        await LoadTodosAsync();
    }

    private async Task LoadTodosAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            await LoadTodosAsync();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadTodosAsync();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadTodosAsync();
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Buy milk");
        item.Title.Should().Be("Buy milk");
        item.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("First");
        await service.AddAsync("Second");
        await service.DeleteAsync(1);
        var all = await service.GetAllAsync();
        all.Should().HaveCount(1);
        all[0].Title.Should().Be("Second");
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Task");
        var items = await service.GetAllAsync();
        var item = items[0];
        await service.ToggleAsync(item.Id);
        var updated = await service.GetAllAsync();
        updated[0].IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void TodoListBase_CallsGetAllAsyncOnInit()
    {
        using var ctx = new Bunit.TestContext();
        var mock = Substitute.For<ITodoService>();
        mock.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));
        ctx.Services.AddSingleton<ITodoService>(mock);
        var cut = ctx.RenderComponent<TodoListBase>();
        mock.Received(1).GetAllAsync();
    }
}