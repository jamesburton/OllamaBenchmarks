using Microsoft.AspNetCore.Components;
using Xunit;

public record TodoItem(int Id, string Title, bool IsCompleted) => GetId();

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
    private int _nextId;

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
        var item = _items.Find(i => i.Id == id);
        if (item is not null)
        {
            item = new TodoItem(item.Id, item.Title, !item.IsCompleted);
            _items.RemoveAll(i => i.Id == item.Id);
            _items.Add(item);
        }
        return Task.CompletedTask;
    }
}

public partial class ComponentTodoList : ComponentBase
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
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        var todo = await TodoService.AddAsync(NewTitle);
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

// xUnit tests
[CollectionDefinition("TodoServiceTests")]
public class TodoServiceTestsCollection { }

[CollectionMember("TodoServiceTests", TodoServiceTestsCollection)]
public class TodoServiceTests : IAsyncLifetime
{
    private ValueTask InitializeAsync() => default;
    private ValueTask DisposeAsync() => default;

    [Fact]
    public async Task AddAsync_CreatesItem_WithCorrectTitle()
    {
        var svc = new TodoService();
        var todo = await svc.AddAsync("New Task");
        Assert.Equal(1, todo.Id);
        Assert.Equal("New Task", todo.Title);
    }

    [Fact]
    public async Task DeleteAsync_REMOVES_Item()
    {
        var svc = new TodoService();
        var item = await svc.AddAsync("Deleted Task");
        var todo = new TodoItem(2, "Deleted Task", false);
        Assert.True(svc._items.Contains(todo));
        await svc.DeleteAsync(1);
        var after = await svc.GetAllAsync();
        Assert.CountEquals(0, after);
    }

    [Fact]
    public async Task ToggleAsync_FLIPPS_Completed()
    {
        var svc = new TodoService();
        svc.NewId++; // avoid conflict
        var item = await svc.AddAsync("Toggle Task");
        Assert.False(item.IsCompleted);
        var todo = new TodoItem(3, "Toggle Task", false);
        Assert.True(svc._items.Contains(todo));
        await svc.ToggleAsync(3);
        var doneItem = await svc.GetAllAsync().First(i => i.Title == "Toggle Task")!;
        Assert.True(doneItem.IsCompleted);
    }
}

// bUnit test
public partial class BunitTestComponentTodoList : Bunit.TestContext
{
    [Test]
    public async Task RenderComponent_Executes_GetAllAsync_OnInit()
    {
        var svc = new TodoService();
        Services.GetOr<ITodoService>(Func.Create(() => svc));
        var cut = RenderComponent<ComponentTodoList>(c => c.TodoService = svc);
        await cut.InvokeAsync("RenderAsync");
        cut.Find("span", i => i.InnerText == "Id 1").ElementShould.BeValid();
    }
}