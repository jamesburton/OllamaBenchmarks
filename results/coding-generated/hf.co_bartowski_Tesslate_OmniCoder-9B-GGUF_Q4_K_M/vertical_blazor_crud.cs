using Microsoft.AspNetCore.Components;
using NSubstitute;
using Xunit;
using Bunit;

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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(_todos.AsReadOnly().ToList());
    }

    public Task<TodoItem> AddAsync(string title)


using Microsoft.AspNetCore.Components;
using NSubstitute;
using Xunit;
using Bunit;

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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(_todos.AsReadOnly().ToList());
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos.Remove(item);
        }
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
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
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        var newItem = await TodoService.AddAsync(NewTitle);
        Todos = await TodoService.GetAllAsync();
        NewTitle = "";
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

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var result = await service.AddAsync("Test Task");
        Assert.Equal("Test Task", result.Title);
        Assert.Equal(1, result.Id);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Task 1");
        await service.AddAsync("Task 2");
        await service.DeleteAsync(1);
        var all = await service.GetAllAsync();
        Assert.Single(all);
        Assert.Equal("Task 2", all[0].Title);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Task 1");
        await service.ToggleAsync(1);
        var all = await service.GetAllAsync();
        Assert.True(all[0].IsCompleted);
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var service = Substitute.For<ITodoService>();
        var todos = new List<TodoItem> { new(1, "Task 1", false) };
        service.GetAllAsync().Returns(Task.FromResult(todos));

        var cut = new TestContext();
        var component = cut.RenderComponent<TodoListBase>();
        await cut.InvokeAsync(() => component.Instance.OnInitializedAsync());

        service.Received(1).GetAllAsync();
    }
}