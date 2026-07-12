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
    private List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return todo;
    }

    public async Task DeleteAsync(int id)
    {
        _todos.RemoveAll(t => t.Id == id);
    }

    public async Task ToggleAsync(int id)
    {
        var todo = _todos.Find(t => t.Id == id);
        todo.IsCompleted = !todo.IsCompleted;
    }
}

// TodoListBase component
public partial class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; }

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        var newTodo = await TodoService.AddAsync(NewTitle);
        Todos.Add(newTodo);
        NewTitle = "";
        await InvokeAsync(StateHasChanged);
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos = await TodoService.GetAllAsync();
        await InvokeAsync(StateHasChanged);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        Todos = await TodoService.GetAllAsync();
        await InvokeAsync(StateHasChanged);
    }
}

// xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var newTodo = await service.AddAsync("New Title");
        Assert.Equal("New Title", newTodo.Title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Title 1");
        await service.AddAsync("Title 2");
        await service.DeleteAsync(1);
        Assert.Single(await service.GetAllAsync());
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var newTodo = await service.AddAsync("Title");
        await service.ToggleAsync(newTodo.Id);
        Assert.True(newTodo.IsCompleted);
    }
}

// bUnit test for TodoListBase
public class TodoListBaseTest
{
    [Fact]
    public async Task Render_CallsGetAllAsyncOnInit()
    {
        var mockService = NSubstitute.Substitute.For<ITodoService>();
        var cut = Bunit.TestContext.RenderComponent<TodoListBase>(p => p.Add(x => x.TodoService, mockService));
        await cut.WaitForAsync();
        mockService.Received().GetAllAsync();
    }
}