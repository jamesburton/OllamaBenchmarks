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

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(new List<TodoItem>(_todos));
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return await Task.FromResult(todo);
    }

    public async Task DeleteAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) _todos.Remove(todo);
        await Task.CompletedTask;
    }

    public async Task ToggleAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) todo.IsCompleted = !todo.IsCompleted;
        await Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
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
        var todo = await TodoService.AddAsync(NewTitle);
        Todos.Add(todo);
        NewTitle = "";
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
        var todo = await service.AddAsync("Test Title");
        todo.Title.Should().Be("Test Title");
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Test Title");
        await service.DeleteAsync(1);
        (await service.GetAllAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Test Title");
        await service.ToggleAsync(1);
        (await service.GetAllAsync())[0].IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var ctx = new Bunit.TestContext();
        var todoService = Substitute.For<ITodoService>();
        var cut = ctx.RenderComponent<TodoListBase>(p => p.Add(x => x.TodoService, todoService));

        await cut.Instance.OnInitializedAsync();

        todoService.Received().GetAllAsync();
    }
}