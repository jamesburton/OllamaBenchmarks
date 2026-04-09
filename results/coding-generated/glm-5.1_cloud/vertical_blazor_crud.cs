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
        var item = _items.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            var index = _items.IndexOf(item);
            _items[index] = item with { IsCompleted = !item.IsCompleted };
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
        var item = await service.AddAsync("Test");
        item.Title.Should().Be("Test");
        item.IsCompleted.Should().BeFalse();
        item.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Test");
        await service.DeleteAsync(1);
        var items = await service.GetAllAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Test");
        await service.ToggleAsync(1);
        var items = await service.GetAllAsync();
        items[0].IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void Renders_And_Calls_GetAllAsync()
    {
        using var ctx = new Bunit.TestContext();
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem>());

        ctx.Services.AddSingleton(mockService);

        var cut = ctx.RenderComponent<TodoListBase>();

        mockService.Received(1).GetAllAsync();
    }
}