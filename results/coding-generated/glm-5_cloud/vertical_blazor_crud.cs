using Microsoft.AspNetCore.Components;

public record TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
}

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
        return Task.FromResult(new List<TodoItem>(_todos));
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem { Id = _nextId++, Title = title };
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        _todos.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _todos.Find(x => x.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
        }
        return Task.CompletedTask;
    }
}

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
    public async Task AddAsync_ShouldCreateItemWithCorrectTitle()
    {
        var service = new TodoService();
        var result = await service.AddAsync("Test Item");

        result.Title.Should().Be("Test Item");
        result.Id.Should().Be(1);
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveItem()
    {
        var service = new TodoService();
        await service.AddAsync("Test");
        await service.DeleteAsync(1);

        var items = await service.GetAllAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleAsync_ShouldFlipIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Test");
        await service.ToggleAsync(1);

        var items = await service.GetAllAsync();
        items.Should().ContainSingle().Which.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_ShouldCallGetAllAsync()
    {
        var todoService = Substitute.For<ITodoService>();
        todoService.GetAllAsync().Returns(new List<TodoItem>());

        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<ITodoService>(todoService);

        var cut = ctx.RenderComponent<TodoListBase>();

        await todoService.Received(1).GetAllAsync();
    }
}