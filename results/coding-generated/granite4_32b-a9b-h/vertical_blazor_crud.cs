using Microsoft.AspNetCore.Components;
using Xunit;
using AwesomeAssertions;
using NSubstitute;
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
    private int _nextId = 1;
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(new List<TodoItem>(_todos));
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return await Task.FromResult(newItem);
    }

    public async Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos.Remove(item);
        }
        await Task.CompletedTask;
    }

    public async Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item = new TodoItem(item.Id, item.Title, !item.IsCompleted);
            int index = _todos.IndexOf(item);
            _todos[index] = item;
        }
        await Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = null!;

    public List<TodoItem> Todos { get; set; } = new();

    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            Todos = await TodoService.GetAllAsync();
        }
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
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();

        var todo = AsyncTestHelper.RunSync(service.AddAsync("Buy milk"));

        todo.Should().Be(new TodoItem(1, "Buy milk", false));
    }

    [Fact]
    public void DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        _ = AsyncTestHelper.RunSync(service.AddAsync("Buy milk"));
        _ = AsyncTestHelper.RunSync(service.AddAsync("Read book"));

        AsyncTestHelper.RunSync(service.DeleteAsync(1));

        service._todos.Should().BeEquivalentTo(new[]
        {
            new TodoItem(2, "Read book", false)
        });
    }

    [Fact]
    public void ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        _ = AsyncTestHelper.RunSync(service.AddAsync("Buy milk"));

        AsyncTestHelper.RunSync(service.ToggleAsync(1));

        service._todos.Should().BeEquivalentTo(new[]
        {
            new TodoItem(1, "Buy milk", true)
        });
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void RendersAndCallsGetAllAsyncOnInit()
    {
        using var ctx = new Bunit.TestContext();
        var mockTodoService = ctx.Mock<ITodoService>();

        // Setup the mock to return a predefined list of todos when GetAllAsync is called
        var initialTodos = new List<TodoItem>
        {
            new TodoItem(1, "Buy milk", false),
            new TodoItem(2, "Read book", true)
        };
        mockTodoService.GetAllAsync().Returns(Task.FromResult(initialTodos));

        // Inject the mocked service into the component under test
        ctx.Services.AddService(mockTodoService);

        var cut = ctx.RenderComponent<TodoListBase>();

        // Verify that GetAllAsync was called on initialization
        mockTodoService.Received(1).GetAllAsync();

        // Additional assertions can be added here to verify rendered output, etc.
    }
}