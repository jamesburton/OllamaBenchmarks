using Microsoft.AspNetCore.Components;

record TodoItem(int Id, string Title, bool IsCompleted);

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

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_items.ToList());

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return item;
    }

    public Task DeleteAsync(int id)
    {
        _items.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null) item.IsCompleted = !item.IsCompleted;
        return Task.CompletedTask;
    }
}

// Blazor component code‑behind
public class TodoListBase : Microsoft.AspNetCore.Components.ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        Todos = await TodoService.GetAllAsync();
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos = await TodoService.GetAllAsync();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        Todos = await TodoService.GetAllAsync();
    }
}

// xUnit v3 tests
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItem_WithCorrectTitle()
    {
        var service = new TodoService();
        var result = await service.AddAsync("Buy milk");
        result.Title.Should().Be("Buy milk");
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Item1");
        await service.AddAsync("Item2");
        await service.DeleteAsync(1);
        var all = await service.GetAllAsync();
        all.Should().Not().Contain(i => i.Id == 1);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Toggle me");
        item.IsCompleted.Should().BeFalse();
        await service.ToggleAsync(item.Id);
        var updated = await service.GetAllAsync();
        updated.First(i => i.Id == item.Id).IsCompleted.Should().BeTrue();
    }
}

// bUnit test
public class TodoListBaseTests
{
    [Fact]
    public void OnInitialized_CallsGetAllAsync()
    {
        var ctx = new Bunit.TestContext();
        var mock = Substitute.For<ITodoService>();
        mock.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));
        ctx.Services.AddSingleton<ITodoService>(mock);
        var cut = ctx.RenderComponent<TodoListBase>();
        mock.Received(1).GetAllAsync();
    }
}