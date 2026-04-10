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
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        _todos.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var index = _todos.FindIndex(t => t.Id == id);
        if (index >= 0)
        {
            var existing = _todos[index];
            _todos[index] = existing with { IsCompleted = !existing.IsCompleted };
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
    public async Task AddAsync_CreatesItem_WithCorrectTitle()
    {
        var service = new TodoService();

        var result = await service.AddAsync("Test Todo");

        result.Title.Should().Be("Test Todo");
        result.IsCompleted.Should().BeFalse();
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Item 1");
        await service.AddAsync("Item 2");

        await service.DeleteAsync(1);
        var todos = await service.GetAllAsync();

        todos.Should().HaveCount(1);
        todos[0].Id.Should().Be(2);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Item 1");

        await service.ToggleAsync(1);
        var todos = await service.GetAllAsync();

        todos[0].IsCompleted.Should().BeTrue();

        await service.ToggleAsync(1);
        todos = await service.GetAllAsync();

        todos[0].IsCompleted.Should().BeFalse();
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public void Component_CallsGetAllAsync_OnInit()
    {
        using var ctx = new Bunit.TestContext();
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem>
        {
            new(1, "Existing Todo", false)
        });

        ctx.Services.AddSingleton<ITodoService>(mockService);

        var cut = ctx.RenderComponent<TodoListBase>();

        mockService.Received().GetAllAsync();
    }
}