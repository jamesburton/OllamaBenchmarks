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
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(_items.ToList());
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        _items.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var index = _items.FindIndex(x => x.Id == id);
        if (index >= 0)
        {
            _items[index] = _items[index] with { IsCompleted = !_items[index].IsCompleted };
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
        var item = await service.AddAsync("Buy milk");
        item.Title.Should().Be("Buy milk");
        item.Id.Should().Be(1);
        item.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("First");
        await service.AddAsync("Second");
        await service.DeleteAsync(1);
        var items = await service.GetAllAsync();
        items.Should().HaveCount(1);
        items[0].Id.Should().Be(2);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Test");
        await service.ToggleAsync(1);
        var items = await service.GetAllAsync();
        items[0].IsCompleted.Should().BeTrue();
        await service.ToggleAsync(1);
        items = await service.GetAllAsync();
        items[0].IsCompleted.Should().BeFalse();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void OnInitialized_CallsGetAllAsync()
    {
        using var ctx = new Bunit.TestContext();
        var todoService = Substitute.For<ITodoService>();
        todoService.GetAllAsync().Returns(new List<TodoItem>());

        ctx.Services.AddSingleton<ITodoService>(todoService);

        var cut = ctx.RenderComponent<TodoListBase>();

        todoService.Received(1).GetAllAsync();
    }
}