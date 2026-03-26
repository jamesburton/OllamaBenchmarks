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
    private List<TodoItem> _items = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return (List<TodoItem>)_items.Clone();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        _items.RemoveAll(x => x.Id == id);
    }

    public async Task ToggleAsync(int id)
    {
        var item = _items.FirstOrDefault(x => x.Id == id);
        if (item != null) item.IsCompleted = !item.IsCompleted;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await TodoService.GetAllAsync().ContinueWith(t => Todos = t.Result);
    }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var item = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        await TodoService.GetAllAsync().ContinueWith(t => Todos = t.Result);
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await TodoService.GetAllAsync().ContinueWith(t => Todos = t.Result);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await TodoService.GetAllAsync().ContinueWith(t => Todos = t.Result);
    }
}

public class TodoServiceTests
{
    [Fact]
    public void AddAsync_CreatesItemWithTitle()
    {
        var service = new TodoService();
        var item = service.AddAsync("Test Title").Result;

        Assert.Equal(1, item.Id);
        Assert.Equal("Test Title", item.Title);
        Assert.False(item.IsCompleted);
    }

    [Fact]
    public void DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var item = service.AddAsync("Test Title").Result;

        service.DeleteAsync(item.Id);
        var items = service.GetAllAsync().Result;

        Assert.Empty(items);
    }

    [Fact]
    public void ToggleAsync_FlipsIsCompletedFlag()
    {
        var service = new TodoService();
        var item = service.AddAsync("Test Title").Result;

        service.ToggleAsync(item.Id);
        var items = service.GetAllAsync().Result;

        Assert.True(items.First(x => x.Id == item.Id).IsCompleted);
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void OnInitializedAsync_LoadsTodos()
    {
        var ctx = new TestContext();
        var todoService = Substitute.For<ITodoService>();
        var component = ctx.RenderComponent<TodoListBase>(p =>
            p.Add(x => x.TodoService, todoService));

        todoService.Received().GetAllAsync();
        Assert.Equal(0, component.Find("li").Count());
    }
}