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
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            var newItem = await TodoService.AddAsync(NewTitle);
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

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = Substitute.For<ITodoService>();
        var todoService = new TodoService();
        var result = await todoService.AddAsync("Test Title");
        result.Title.Should().Be("Test Title");
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = Substitute.For<ITodoService>();
        var todoService = new TodoService();
        await todoService.AddAsync("Test Title");
        await todoService.DeleteAsync(1);
        var todos = await todoService.GetAllAsync();
        todos.Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = Substitute.For<ITodoService>();
        var todoService = new TodoService();
        await todoService.AddAsync("Test Title");
        await todoService.ToggleAsync(1);
        var todos = await todoService.GetAllAsync();
        todos[0].IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void TodoListBase_CallsGetAllAsyncOnInit()
    {
        var mockService = Substitute.For<ITodoService>();
        var cut = new TodoListBase { TodoService = mockService };
        cut.OnInitializedAsync();
        mockService.Received().GetAllAsync();
    }
}