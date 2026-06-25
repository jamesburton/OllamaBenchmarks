public record TodoItem(int Id, string Title, bool IsCompleted);

// Interface definition
public interface ITodoService 
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// Service implementation
public class TodoService : ITodoService
{
    private readonly List<TodoItem> _todos = new();
    private int _nextId;

    public async Task<List<TodoItem>> GetAllAsync()
        => await Task.FromResult(_todos.ToList());

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(++_nextId, title, false);
        _todos.Add(todo);
        return todo;
    }

    public async Task DeleteAsync(int id) 
        => await Task.Run(() => _todos.RemoveAll(t => t.Id == id));

    public async Task ToggleAsync(int id)
        => await Task.Run(() => 
            _todos.First(t => t.Id == id).IsCompleted = !t.IsCompleted);
}

// Component code-behind
public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; }

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
        => await LoadTodos();

    private async Task LoadTodos() 
        => Todos = (await TodoService.GetAllAsync()).ToList();

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            var todo = await TodoService.AddAsync(NewTitle);
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

// xUnit tests
public class TodoServiceTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();

        var todo = await service.AddAsync("Test Title");

        Assert.Equal(1, todo.Id);
        Assert.Equal("Test Title", todo.Title);
        Assert.False(todo.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var todo = await service.AddAsync("Test");

        await service.DeleteAsync(todo.Id);

        var todos = (await service.GetAllAsync()).ToList();
        Assert.Empty(todos);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var todo = await service.AddAsync("Test");

        await service.ToggleAsync(todo.Id);

        var updatedTodo = (await service.GetAllAsync()).First(t => t.Id == todo.Id);
        Assert.True(updatedTodo.IsCompleted);
    }
}

// bUnit test
public class TodoListBaseTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task OnInitialized_CallsGetAllAsync()
    {
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

        using (var ctx = new Bunit.TestContext())
        {
            ctx.Services.Add(ServiceDescriptor.Singleton(mockService));

            var cut = ctx.RenderComponent<TodoListBase>();

            await mockService.Received(1).GetAllAsync();
        }
    }
}