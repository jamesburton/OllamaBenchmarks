using Microsoft.AspNetCore.Components;
using Bunit;
using NSubstitute;

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
    private int _nextId = 1; // instance‑scoped auto‑increment

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_items.OrderBy(i => i.Id).ToList());

    public Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _items.Add(todo);
        return Task.FromResult(todo);
    }

    public Task DeleteAsync(int id)
    {
        _items.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var todo = _items.FirstOrDefault(i => i.Id == id);
        if (todo != null) todo.IsCompleted = !todo.IsCompleted;
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
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        var todo = await TodoService.AddAsync(NewTitle);
        Todos.Add(todo);
        Todos = await TodoService.GetAllAsync();
        NewTitle = "";
    }

    public async Task DeleteTodo(int id)
    {
        TodoService.DeleteAsync(id).Wait();
        Todos = await TodoService.GetAllAsync();
    }

    public async Task ToggleTodo(int id)
    {
        TodoService.ToggleAsync(id).Wait();
        Todos = await TodoService.GetAllAsync();
    }
}

// xUnit v3 tests for TodoService
[Fact] public void AddAsync_CreatesItem_WithCorrectTitle()
{
    var svc = new TodoService();
    var result = svc.AddAsync("Test title").GetAwaiter().GetResult();
    Assert.Equal("Test title", result!.Title);
    var all = svc.GetAllAsync().GetAwaiter().GetResult();
    Assert.Equal(1, all.Count);
    var it = all[0];
    Assert.Equal(it.Id, it.Id);
}

[Fact] public async void DeleteAsync_RemovesItem()
{
    var svc = new TodoService();
    var created = await svc.AddAsync("Delete it");
    svc.DeleteAsync(created.Id).Wait();

    var all = await svc.GetAllAsync();
    Assert.Equal(0, all.Count);
}

[Fact] public async void ToggleAsync_FlipsIsCompleted()
{
    // add a few items
    var svc = new TodoService();
    var created = await svc.AddAsync("Toggle item");
    svc.AddAsync("Another");
    svc.AddAsync("Yet another");

    // ensure the created item is not completed
    Assert.False(created.IsCompleted);

    // create a clone to mutate
    var cloned = new TodoService();
    cloned.AddAsync("X").GetAwaiter().GetResult();
    cloned.AddAsync("Y").GetAwaiter().GetResult();

    cloned.ToggleAsync(created.Id).Wait();

    Assert.True(cloned.GetAllAsync().GetAwaiter().GetResult()[0].IsCompleted);
}

// bUnit test for TodoListBase – verifies GetAllAsync is called on init
public class TodoListBase_Bunit_test
{
    [Test] public async Task Rendering_Calls_GetAllAsync()
    {
        // arrange
        var svcMock = Substitute.For<ITodoService>();
        svcMock.GetAllAsync(Returns(t => List.Empty<TodoItem>()));

        var tlc = TestContext.Current.GetcomponentRef<TodoListBase>();
        tlc.TodoService = svcMock;

        // act
        TodoListBase.OnInitializedAsync().GetAwaiter().GetResult();

        // assert
        svcMock.MustHaveBeenCalledOnceOnce().GetAllAsync(t => t.Arg<List<TodoItem>>()?.Count == 0);
    }
}