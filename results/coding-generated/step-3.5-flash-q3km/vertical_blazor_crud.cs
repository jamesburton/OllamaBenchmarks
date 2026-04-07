public record TodoItem(int Id, string Title, bool IsCompleted);

public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// Service Implementation
public class TodoService : ITodoService
{
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(_todos.ToList());
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return Task.FromResult(todo);
    }

    public Task DeleteAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) _todos.Remove(todo);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            _todos.Remove(todo);
            _todos.Add(todo with { IsCompleted = !todo.IsCompleted });
        }
        return Task.CompletedTask;
    }
}

// Component
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadTodos();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            await LoadTodos();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadTodos();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadTodos();
    }

    private async Task LoadTodos()
    {
        Todos = await TodoService.GetAllAsync();
    }
}

// xUnit Tests for TodoService
using Xunit;
using AwesomeAssertions;

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var result = await service.AddAsync("Test task");
        result.Title.Should().Be("Test task");
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var added = await service.AddAsync("To delete");
        var initialCount = (await service.GetAllAsync()).Count;

        await service.DeleteAsync(added.Id);
        var finalCount = (await service.GetAllAsync()).Count;

        finalCount.Should().Be(initialCount - 1);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var added = await service.AddAsync("Toggle me");
        added.IsCompleted.Should().BeFalse();

        await service.ToggleAsync(added.Id);
        var updated = (await service.GetAllAsync()).First(t => t.Id == added.Id);
        updated.IsCompleted.Should().BeTrue();

        await service.ToggleAsync(added.Id);
        var toggledBack = (await service.GetAllAsync()).First(t => t.Id == added.Id);
        toggledBack.IsCompleted.Should().BeFalse();
    }
}

// bUnit Test for TodoListBase
using Bunit;
using NSubstitute;

public class TodoListBaseTests
{
    [Fact]
    public void Component_CallsGetAllAsyncOnInit()
    {
        var bunitTestContext = new Bunit.TestContext();
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

        var cut = bunitTestContext.RenderComponent<TodoListBase>(parameters => parameters
            .Add(x => x.TodoService, mockService));

        mockService.Received(1).GetAllAsync();
    }
}