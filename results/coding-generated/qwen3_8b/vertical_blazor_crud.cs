using Microsoft.AspNetCore.Components;

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
        return new List<TodoItem>(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        var item = _todos.Find(t => t.Id == id);
        if (item != null)
            _todos.Remove(item);
    }

    public async Task ToggleAsync(int id)
    {
        var item = _todos.Find(t => t.Id == id);
        if (item != null)
            item.IsCompleted = !item.IsCompleted;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;
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
    public async Task TestAddAsync_CreatesItemWithCorrectTitle()
    {
        var service = Substitute.For<ITodoService>();
        var todoService = new TodoService();

        var result = await todoService.AddAsync("Test Title");

        result.Title.Should().Be("Test Title");
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task TestDeleteAsync_RemovesItem()
    {
        var service = Substitute.For<ITodoService>();
        var todoService = new TodoService();
        await todoService.AddAsync("Test Title");
        await todoService.DeleteAsync(1);

        var todos = await todoService.GetAllAsync();
        todos.Should().BeEmpty();
    }

    [Fact]
    public async Task TestToggleAsync_FlipsIsCompleted()
    {
        var service = Substitute.For<ITodoService>();
        var todoService = new TodoService();
        var item = await todoService.AddAsync("Test Title");
        await todoService.ToggleAsync(item.Id);

        var updated = await todoService.GetAllAsync().FirstAsync();
        updated.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task TestTodoListBase_LoadsTodosOnInit()
    {
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem>
        {
            new(1, "Task 1", false),
            new(2, "Task 2", true)
        });

        var cut = Bunit.TestContext.RenderComponent<TodoListBase>(p => p.Add(x => x.TodoService, mockService));

        cut.Instance.Todos.Should().HaveCount(2);
        cut.Instance.Todos.Should().Contain(t => t.Id == 1 && t.Title == "Task 1");
        cut.Instance.Todos.Should().Contain(t => t.Id == 2 && t.Title == "Task 2");
    }
}