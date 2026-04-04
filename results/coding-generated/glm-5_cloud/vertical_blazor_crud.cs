using Microsoft.AspNetCore.Components;

// 1. TodoItem record
public record TodoItem(int Id, string Title, bool IsCompleted);

// 2. ITodoService interface
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// 3. TodoService implementation
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
        if (todo is not null)
        {
            _todos.Remove(todo);
        }
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var index = _todos.FindIndex(t => t.Id == id);
        if (index >= 0)
        {
            _todos[index] = _todos[index] with { IsCompleted = !_todos[index].IsCompleted };
        }
        return Task.CompletedTask;
    }
}

// 4. TodoListBase component
public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

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

// 5. xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();

        var todo = await service.AddAsync("Test Todo");

        todo.Title.Should().Be("Test Todo");
        todo.Id.Should().Be(1);
        todo.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Test 1");
        await service.AddAsync("Test 2");

        await service.DeleteAsync(1);

        var todos = await service.GetAllAsync();
        todos.Should().HaveCount(1);
        todos[0].Id.Should().Be(2);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Test");

        await service.ToggleAsync(1);

        var todos = await service.GetAllAsync();
        todos[0].IsCompleted.Should().BeTrue();
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public void OnInitializedAsync_CallsGetAllAsync()
    {
        using var ctx = new Bunit.TestContext();
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem>());

        ctx.Services.AddSingleton<ITodoService>(mockService);

        var cut = ctx.RenderComponent<TodoListBase>();

        mockService.Received().GetAllAsync();
    }
}