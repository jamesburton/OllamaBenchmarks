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
    private int _nextId = 1;
    private readonly List<TodoItem> _todos = new();

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(_todos.ToList());
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return Task.FromResult(todo);
    }

    public Task DeleteAsync(int id)
    {
        var todoToRemove = _todos.FirstOrDefault(t => t.Id == id);
        if (todoToRemove != null)
        {
            _todos.Remove(todoToRemove);
        }
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var todoToToggle = _todos.FirstOrDefault(t => t.Id == id);
        if (todoToToggle != null)
        {
            todoToToggle.IsCompleted = !todoToToggle.IsCompleted;
        }
        return Task.CompletedTask;
    }
}

// TodoListBase ComponentBase
public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
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

// xUnit tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var todo = await service.AddAsync("Test Todo");
        todo.Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var todo1 = await service.AddAsync("Todo 1");
        var todo2 = await service.AddAsync("Todo 2");

        await service.DeleteAsync(todo1.Id);

        var todos = await service.GetAllAsync();
        todos.Should().NotContain(todo1);
        todos.Should().Contain(todo2);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var todo = await service.AddAsync("Test Todo");

        await service.ToggleAsync(todo.Id);

        var updatedTodo = await service.GetAllAsync().FirstOrDefaultAsync(t => t.Id == todo.Id);
        updatedTodo.Should().NotBeNull();
        updatedTodo.IsCompleted.Should().BeTrue();

        await service.ToggleAsync(todo.Id);
        updatedTodo = await service.GetAllAsync().FirstOrDefaultAsync(t => t.Id == todo.Id);
        updatedTodo.Should().NotBeNull();
        updatedTodo.IsCompleted.Should().BeFalse();
    }
}

// bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var mockService = Substitute.For<ITodoService>();
        var ctx = new Bunit.TestContext();
        var component = ctx.RenderComponent<TodoListBase>();

        await component.InvokeAsync(async () =>
        {
            await component.OnInitializedAsync();
        });

        mockService.Received().GetAllAsync();
    }
}