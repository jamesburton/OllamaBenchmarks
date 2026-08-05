using Microsoft.AspNetCore.Components;
using NSubstitute;
using Xunit;
using AwesomeAssertions;

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
    private int _nextId;

    public TodoService()
    {
        _nextId = 1;
    }

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.ToList());

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(Id: _nextId++, Title: title, IsCompleted: false);
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id) => Task.Run(() => _todos.RemoveAll(t => t.Id == id));

    public Task ToggleAsync(int id) => Task.Run(() =>
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null) item.IsCompleted = !item.IsCompleted;
    });
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
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        var newItem = await TodoService.AddAsync(NewTitle);
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
    public void AddAsync_creates_item_with_correct_title()
    {
        var service = new TodoService();
        Assert.Throws<NullReferenceException>(() => service.AddAsync("")); // ensure empty title handled gracefully or throw; here we just verify non-empty works.

        var item = service.AddAsync("Buy milk").Result;
        item.Should().NotBeNull();
        item.Title.Should().Be("Buy milk");
    }

    [Fact]
    public void DeleteAsync_removes_item()
    {
        var service = new TodoService();
        service.AddAsync("A").GetAwaiter().GetResult();
        service.AddAsync("B").GetAwaiter().GetResult();
        service.DeleteAsync(2).GetAwaiter().GetResult();

        var list = service.GetAllAsync().Result;
        list.Should().ContainSingle(t => t.Title == "A");
    }

    [Fact]
    public void ToggleAsync_flips_IsCompleted()
    {
        var service = new TodoService();
        var item = service.AddAsync("Toggle me").GetAwaiter().GetResult();
        Assert.False(item.IsCompleted);

        service.ToggleAsync(item.Id).GetAwaiter().GetResult();
        var updated = service.GetAllAsync().Result.First(t => t.Id == item.Id);
        updated.Should().BeTrue(t => t.IsCompleted);
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task Render_component_calls_GetAllAsync_on_init()
    {
        var ctx = new Bunit.TestContext();
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem>());

        await ctx.RenderComponent<TodoListBase>(p => p.WithParameter("TodoService", service));

        service.Received(1).GetAllAsync();
    }
}