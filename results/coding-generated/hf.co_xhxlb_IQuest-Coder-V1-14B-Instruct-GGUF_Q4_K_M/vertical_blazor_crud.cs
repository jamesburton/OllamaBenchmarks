using Microsoft.AspNetCore.Components;
using AwesomeAssertions;
using NSubstitute;
using Bunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

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

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(new List<TodoItem>(_todos));
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return Task.FromResult(todo);
    }

    public Task DeleteAsync(int id)
    {
        var todo = _todos.Find(t => t.Id == id);
        if (todo != null)
        {
            _todos.Remove(todo);
        }
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var todo = _todos.Find(t => t.Id == id);
        if (todo != null)
        {
            todo.IsCompleted = !todo.IsCompleted;
        }
        return Task.CompletedTask;
    }
}

public partial class TodoListBase : ComponentBase
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

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        // Arrange
        var service = new TodoService();

        // Act
        var result = await service.AddAsync("Test Todo");

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Todo");
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = new TodoService();
        await service.AddAsync("Test Todo");
        var todos = await service.GetAllAsync();
        var idToDelete = todos[0].Id;

        // Act
        await service.DeleteAsync(idToDelete);
        var remainingTodos = await service.GetAllAsync();

        // Assert
        remainingTodos.Should().HaveCount(0);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        // Arrange
        var service = new TodoService();
        await service.AddAsync("Test Todo");
        var todos = await service.GetAllAsync();
        var idToToggle = todos[0].Id;

        // Act
        await service.ToggleAsync(idToToggle);
        var updatedTodos = await service.GetAllAsync();
        var toggledTodo = updatedTodos.Find(t => t.Id == idToToggle);

        // Assert
        toggledTodo.Should().NotBeNull();
        toggledTodo!.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void Test_OnInitializedAsync_CallsGetAllAsync()
    {
        // Arrange
        var mockService = Substitute.For<ITodoService>();
        var cut = new TodoListBase { TodoService = mockService };

        // Act
        cut.OnInitializedAsync().GetAwaiter().GetResult();

        // Assert
        mockService.Received().GetAllAsync();
    }
}