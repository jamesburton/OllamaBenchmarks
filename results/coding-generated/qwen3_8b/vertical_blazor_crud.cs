using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Record for TodoItem
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
        return new List<TodoItem>(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return todo;
    }

    public async Task DeleteAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            _todos.Remove(todo);
        }
    }

    public async Task ToggleAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            todo.IsCompleted = !todo.IsCompleted;
        }
    }
}

// TodoListBase component
public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

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
            var todo = await TodoService.AddAsync(NewTitle);
            Todos = await TodoService.GetAllAsync();
            NewTitle = "";
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

// xUnit v3 tests for TodoService
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
        result.IsCompleted.Should().BeFalse();
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
        todos.First().IsCompleted.Should().BeTrue();
    }
}

// bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public async Task TodoListBase_LoadsTodosOnInit()
    {
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem>
        {
            new(1, "Todo 1", false),
            new(2, "Todo 2", true)
        });

        var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<TodoListBase>(p => p.Inject(service));

        await cut.Instance.OnInitializedAsync();

        cut.Instance.Todos.Should().HaveCount(2);
        cut.Instance.Todos[0].Title.Should().Be("Todo 1");
        cut.Instance.Todos[1].Title.Should().Be("Todo 2");
    }
}