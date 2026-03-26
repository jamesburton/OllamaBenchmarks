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
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item is null) throw new InvalidOperationException($"Item with id {id} not found");
        var newItem = new TodoItem(item.Id, item.Title, !item.IsCompleted);
        _items.Remove(item);
        _items.Add(newItem);
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

// xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var result = service.AddAsync("Test Title").Result;
        result.Title.Should().Be("Test Title");
        result.Id.Should().Be(1);
    }

    [Fact]
    public void DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        service.AddAsync("A").Result;
        service.AddAsync("B").Result;
        service.DeleteAsync(1).Result;
        var all = service.GetAllAsync().Result;
        all.Should().HaveCount(1);
        all[0].Title.Should().Be("B");
    }

    [Fact]
    public void ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = service.AddAsync("Task").Result;
        bool initial = item.IsCompleted;
        service.ToggleAsync(item.Id).Result();
        var refreshed = service.GetAllAsync().Result.First(i => i.Id == item.Id);
        refreshed.IsCompleted.Should().Be(!initial);
    }
}

// bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public void RenderComponent_Calls_GetAllAsync_OnInit()
    {
        // Arrange
        var ctx = new Bunit.TestContext();
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem> { new TodoItem(1, "Item1", false) });

        // Act
        var cut = ctx.RenderComponent<TestComponent>(params =>
        {
            params.Add(x => x.TodoService, mockService);
        });

        // Assert
        mockService.Received(1).GetAllAsync();
    }

    // Helper component used only for rendering in the test
    private class TestComponent : TodoListBase { }
}