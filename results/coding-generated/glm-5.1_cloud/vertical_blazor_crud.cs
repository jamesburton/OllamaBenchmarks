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
            _todos[index] = _todos[index] with { IsCompleted = !_todos[index].IsCompleted };
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

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();

        var item = await service.AddAsync("Test todo");

        item.Title.Should().Be("Test todo");
        item.IsCompleted.Should().BeFalse();
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
        await service.AddAsync("Test");

        await service.ToggleAsync(1);

        var all = await service.GetAllAsync();
        all[0].IsCompleted.Should().BeTrue();

        await service.ToggleAsync(1);

        all = await service.GetAllAsync();
        all[0].IsCompleted.Should().BeFalse();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void OnInitialized_CallsGetAllAsync()
    {
        using var ctx = new Bunit.TestContext();
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem>());

        ctx.Services.AddSingleton<ITodoService>(mockService);

        var cut = ctx.RenderComponent<TodoListBase>();

        mockService.Received(1).GetAllAsync();
    }
}