using Microsoft.AspNetCore.Components;

// TodoItem record
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
    private List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return new List<TodoItem>(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return newItem;
    }

    public async Task DeleteAsync(int id)
    {
        var item = _todos.Find(t => t.Id == id);
        if (item != null)
            _todos.Remove(item);
    }

    public async Task ToggleAsync(int id)
    {
        var item = _None(_todos, t => t.Id == id);
        if (item != null)
            item.IsCompleted = !item.IsCompleted;
    }

    private T _None<T>(List<T> list, Func<T, bool> predicate)
    {
        return list.Find(predicate);
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

// xUnit tests for TodoService
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
        var item = await todoService.AddAsync("Test Title");

        await todoService.ToggleAsync(item.Id);
        var todos = await todoService.GetAllAsync();

        todos.First().IsCompleted.Should().BeTrue();
    }
}

// bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public async Task TodoListBase_CallsGetAllAsyncOnInit()
    {
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem> { new(1, "Test", false) });

        var cut = Bunit.TestContext.RenderComponent<TodoListBase>(p => p.Add(x => x.TodoService, mockService));

        await mockService.Received(1).GetAllAsync();
    }
}