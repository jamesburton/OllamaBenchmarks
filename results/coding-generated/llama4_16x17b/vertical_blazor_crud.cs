using Microsoft.AspNetCore.Components;
using NSubstitute;

// 1. TodoItem record
public record TodoItem(int Id, string Title, bool IsCompleted);

// 2. ITodoService interface
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// 3. TodoService implementation
public class TodoService : ITodoService
{
    private int _idCounter = 0;
    private List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_idCounter++, title, false);
        _todos.Add(todo);
        return todo;
    }

    public async Task DeleteAsync(int id)
    {
        _todos = _todos.Where(t => t.Id != id).ToList();
    }

    public async Task ToggleAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            todo = new TodoItem(todo.Id, todo.Title, !todo.IsCompleted);
            _todos[_todos.IndexOf(_todos.First(t => t.Id == id))] = todo;
        }
    }
}

// 4. TodoListBase component
public partial class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadTodosAsync();
    }

    public async Task LoadTodosAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodoAsync()
    {
        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        await LoadTodosAsync();
    }

    public async Task DeleteTodoAsync(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadTodosAsync();
    }

    public async Task ToggleTodoAsync(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadTodosAsync();
    }
}

// 5. xUnit tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        // Arrange
        var service = new TodoService();

        // Act
        var todo = await service.AddAsync("Test");

        // Assert
        todo.Title.Should().Be("Test");
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = new TodoService();
        await service.AddAsync("Test");

        // Act
        await service.DeleteAsync(0);

        // Assert
        (await service.GetAllAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleAsync_TogglesIsCompleted()
    {
        // Arrange
        var service = new TodoService();
        await service.AddAsync("Test");

        // Act
        await service.ToggleAsync(0);

        // Assert
        (await service.GetAllAsync()).First().IsCompleted.Should().BeTrue();
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public async Task RendersCorrectly()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem>());

        // Act
        var ctx = new TestContext();
        var cut = ctx.RenderComponent<TodoListBase>();

        // Assert
        cut.Markup.Should().NotBeNull();
    }
}