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
        return Task.FromResult(new List<TodoItem>(_todos));
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
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
        var index = _todos.FindIndex(x => x.Id == id);
        if (index >= 0)
        {
            var old = _todos[index];
            _todos[index] = old with { IsCompleted = !old.IsCompleted };
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
        await LoadTodos();
    }

    private async Task LoadTodos()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            await LoadTodos();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadTodos();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadTodos();
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var title = "Test Todo";

        var result = await service.AddAsync(title);

        result.Title.Should().Be(title);
        result.Id.Should().Be(1);
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test");
        var initialCount = (await service.GetAllAsync()).Count;

        await service.DeleteAsync(item.Id);
        var finalCount = (await service.GetAllAsync()).Count;

        finalCount.Should().Be(initialCount - 1);
    }

    [Fact]
    public async Task ToggleAsync_FlippedIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test");
        var initialCompleted = item.IsCompleted;

        await service.ToggleAsync(item.Id);
        var updatedList = await service.GetAllAsync();
        var updatedItem = updatedList.First(x => x.Id == item.Id);

        updatedItem.IsCompleted.Should().Be(!initialCompleted);
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void Component_CallsGetAllAsyncOnInit()
    {
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem>());

        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(mockService);

        var cut = ctx.RenderComponent<TodoListBase>();

        mockService.Received(1).GetAllAsync();
    }
}