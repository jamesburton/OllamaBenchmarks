public record TodoItem(int Id, string Title, bool IsCompleted);

// ITodoService.cs
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// TodoService.cs
public class TodoService : ITodoService
{
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos.ToList());
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return await Task.FromResult(todo);
    }

    public async Task DeleteAsync(int id)
    {
        _todos.RemoveAll(t => t.Id == id);
        await Task.CompletedTask;
    }

    public async Task ToggleAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            todo.IsCompleted = !todo.IsCompleted;
        }
        await Task.CompletedTask;
    }
}

// TodoListBase.cs
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
        if (!string.IsNullOrEmpty(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            Todos = await TodoService.GetAllAsync();
        }
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

// TodoServiceTests.cs
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
        var todo = await service.AddAsync("Test Title");
        await service.DeleteAsync(todo.Id);
        var todos = await service.GetAllAsync();
        todos.Should().NotContain(t => t.Id == todo.Id);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var todo = await service.AddAsync("Test Title");
        await service.ToggleAsync(todo.Id);
        var updatedTodo = await service.GetAllAsync().First(t => t.Id == todo.Id);
        updatedTodo.IsCompleted.Should().BeTrue();
    }
}

// TodoListBaseTests.cs
public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var service = Substitute.For<ITodoService>();
        var todo = new TodoItem(1, "Test", false);
        service.GetAllAsync().ReturnsAsync(new List<TodoItem> { todo });

        var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<TodoListBase>();
        cut.Instance.TodoService = service;

        await cut.Instance.OnInitializedAsync();
        service.Received(1).GetAllAsync();
    }
}