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
    private int _nextId = 1;
    private readonly List<TodoItem> _items = new();

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_items.ToList());

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        _items.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var target = _items.Find(i => i.Id == id);
        if (target != null) target.IsCompleted = !target.IsCompleted;
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
        Todos = (await TodoService.GetAllAsync()).ToList();
    }

    public async Task AddTodo()
    {
        var added = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        Todos = (await TodoService.GetAllAsync()).ToList();
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos = (await TodoService.GetAllAsync()).ToList();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        Todos = (await TodoService.GetAllAsync()).ToList();
    }
}

// xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItem_WithCorrectTitle()
    {
        var service = new TodoService();
        var result = await service.AddAsync("Test Title");
        result.Title.Should().Be("Test Title");
        result.Id.Should().Be(1);
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Item1");
        await service.AddAsync("Item2");
        await service.DeleteAsync(1);
        var all = await service.GetAllAsync();
        all.Should().HaveCount(1);
        all[0].Title.Should().Be("Item2");
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Toggle Me");
        await service.ToggleAsync(item.Id);
        var refreshed = await service.GetAllAsync();
        var toggled = refreshed.Single(i => i.Id == item.Id);
        toggled.IsCompleted.Should().BeTrue();
    }
}

// bUnit test for TodoListBase using NSubstitute
public class TodoListBaseTests
{
    [Fact]
    public void TodoListBase_RendersAndCallsGetAllAsyncOnInit()
    {
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(Task.FromResult(new List<TodoItem> { new TodoItem(1, "Test", false) }));

        var ctx = new Bunit.TestContext();
        ctx.RenderComponent<TodoListBase>();

        service.Received(1).GetAllAsync();
    }
}