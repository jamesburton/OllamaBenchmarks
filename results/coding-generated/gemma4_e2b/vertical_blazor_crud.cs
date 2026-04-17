using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Bunit;
using AwesomeAssertions;
using Microsoft.AspNetCore.Components;

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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        // Return a copy of the list
        return Task.FromResult(_todos.ToList());
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return Task.FromResult(newItem);
    }

    public Task DeleteAsync(int id)
    {
        _todos.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
        }
        return Task.CompletedTask;
    }
}

// 4. TodoListBase ComponentBase code-behind class
public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; }
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadTodos();
    }

    private async Task LoadTodos()
    {
        var todos = await TodoService.GetAllAsync();
        Todos = todos;
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            var newItem = await TodoService.AddAsync(NewTitle);
            Todos.Add(newItem);
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

// --- xUnit v3 Tests for TodoService ---
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var result = await service.AddAsync("Buy milk");

        Assert.Equal("Buy milk", result.Title);
        Assert.Equal(1, result.Id); // Since the service starts ID from 1
        Assert.Single(await service.GetAllAsync());
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Item to delete");
        await service.AddAsync("Another item");

        var all = await service.GetAllAsync();
        var itemToDelete = all.First(t => t.Title == "Item to delete");

        await service.DeleteAsync(itemToDelete.Id);

        var remaining = await service.GetAllAsync();
        Assert.Single(remaining);
        Assert.DoesNotContain(remaining, t => t.Id == itemToDelete.Id);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Toggle me");
        var initialTodos = await service.GetAllAsync();
        var todoToToggle = initialTodos.First();

        await service.ToggleAsync(todoToToggle.Id);

        var updatedTodos = await service.GetAllAsync();
        var updatedTodo = updatedTodos.First(t => t.Id == todoToToggle.Id);

        Assert.False(todoToToggle.IsCompleted); // Should be flipped from false to true
        Assert.True(updatedTodo.IsCompleted);
    }
}

// --- bUnit Test for TodoListBase ---
public class TodoListBaseTests : IAsyncLifetime
{
    private ITodoService _mockTodoService;
    private TodoListBase _component;

    public async Task InitializeAsync()
    {
        // Setup NSubstitute mock
        _mockTodoService = Substitute.For<ITodoService>();

        // Setup initial state for the mock service
        _mockTodoService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));
        _mockTodoService.AddAsync(Arg.Any<string>()).Returns(Task.FromResult(new TodoItem(1, "Mock Item", false)));
        _mockTodoService.DeleteAsync(Arg.Any<int>()).Returns(Task.CompletedTask);
        _mockTodoService.ToggleAsync(Arg.Any<int>()).Returns(Task.CompletedTask);

        // Setup the component instance
        _component = new TodoListBase();
        _component.TodoService = _mockTodoService;
    }

    public async Task DisposeAsync()
    {
        // Cleanup if necessary
    }

    [Fact]
    public async Task OnInitializedAsync_LoadsTodosFromService()
    {
        // Arrange: Mock the service to return data on initialization
        var mockTodos = new List<TodoItem>
        {
            new TodoItem(1, "Task A", false),
            new TodoItem(2, "Task B", true)
        };
        _mockTodoService.GetAllAsync().Returns(Task.FromResult(mockTodos));

        // Act
        await _component.OnInitializedAsync();

        // Assert
        // Verify that GetAllAsync was called exactly once during initialization
        await _mockTodoService.Received(1).GetAllAsync();
        // Verify that the component's internal list was populated correctly
        Assert.Equal(mockTodos, _component.Todos);
    }

    [Fact]
    public async Task AddTodo_CallsServiceAndUpdatesList()
    {
        // Arrange
        var newItem = new TodoItem(99, "New Task", false);
        _mockTodoService.AddAsync(newItem.Title).Returns(Task.FromResult(newItem));

        // Act
        _component.NewTitle = "Test Add";
        await _component.AddTodo();

        // Assert
        // Verify that AddAsync was called with the correct title
        await _mockTodoService.Received(1).AddAsync("Test Add");
        // Verify that the list was reloaded after adding
        await _mockTodoService.Received(1).GetAllAsync();
        Assert.Single(_component.Todos);
        Assert.Equal(newItem, _component.Todos.First());
        Assert.Equal("Test Add", _component.NewTitle); // Check if NewTitle was cleared
    }

    [Fact]
    public async Task DeleteTodo_CallsServiceAndUpdatesList()
    {
        // Arrange
        var todoIdToDelete = 1;
        _mockTodoService.DeleteAsync(todoIdToDelete).Returns(Task.CompletedTask);

        // Setup initial state
        var initialTodos = new List<TodoItem> { new TodoItem(todoIdToDelete, "Delete Me", false) };
        _mockTodoService.GetAllAsync().Returns(Task.FromResult(initialTodos));
        _component.Todos = initialTodos; // Manually set initial state for this test context

        // Act
        await _component.DeleteTodo(todoIdToDelete);

        // Assert
        // Verify that DeleteAsync was called
        await _mockTodoService.Received(1).DeleteAsync(todoIdToDelete);
        // Verify that the list was reloaded after deletion
        await _mockTodoService.Received(1).GetAllAsync();
        Assert.Empty(_component.Todos);
    }

    [Fact]
    public async Task ToggleTodo_CallsServiceAndUpdatesList()
    {
        // Arrange
        var todoIdToToggle = 1;
        var initialTodos = new List<TodoItem>
        {
            new TodoItem(todoIdToToggle, "Toggle Me", false)
        };
        _mockTodoService.GetAllAsync().Returns(Task.FromResult(initialTodos));
        _mockTodoService.ToggleAsync(todoIdToToggle).Returns(Task.CompletedTask);

        // Act
        await _component.ToggleTodo(todoIdToToggle);

        // Assert
        // Verify that ToggleAsync was called
        await _mockTodoService.Received(1).ToggleAsync(todoIdToToggle);
        // Verify that the list was reloaded after toggling
        await _mockTodoService.Received(1).GetAllAsync();
        var updatedTodos = await _mockTodoService.GetAllAsync();
        Assert.Single(updatedTodos);
        Assert.True(updatedTodos.First().IsCompleted);
    }
}