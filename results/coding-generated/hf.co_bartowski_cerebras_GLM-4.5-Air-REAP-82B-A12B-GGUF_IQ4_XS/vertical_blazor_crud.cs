public record TodoItem(int Id, string Title, bool IsCompleted);

// ITodoService interface
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// TodoService implementation
public class TodoService : ITodoService
{
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        await Task.Delay(0); // Simulate async operation
        return _todos.ToList(); // Return a copy
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        await Task.Delay(0);
        return todo;
    }

    public async Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null) _todos.Remove(item);
        await Task.Delay(0);
    }

    public async Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos[_todos.IndexOf(item)] = item with { IsCompleted = !item.IsCompleted };
        }
        await Task.Delay(0);
    }
}

// TodoListBase component
public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadTodos();
    }

    private async Task LoadTodos() => Todos = await TodoService.GetAllAsync();

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

// xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var todo = service.AddAsync("Test").Result;

        todo.Should().NotBeNull();
        todo.Title.Should().Be("Test");
        todo.Id.Should().Be(1);
    }

    [Fact]
    public void DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        service.AddAsync("First").Wait();
        service.AddAsync("Second").Wait();

        service.DeleteAsync(1).Wait();
        service.GetAllAsync().Result.Should().HaveCount(1);
        service.GetAllAsync().Result.First().Title.Should().Be("Second");
    }

    [Fact]
    public void ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var todo = service.AddAsync("Test").Result;

        service.ToggleAsync(todo.Id).Wait();
        service.GetAllAsync().Result.First().IsCompleted.Should().BeTrue();

        service.ToggleAsync(todo.Id).Wait();
        service.GetAllAsync().Result.First().IsCompleted.Should().BeFalse();
    }
}

// bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public void ComponentCallsGetAllOnInit()
    {
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem>());

        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<TodoListBase>(p => 
            p.Add(x => x.TodoService, mockService));

        mockService.Received(1).GetAllAsync();
    }
}