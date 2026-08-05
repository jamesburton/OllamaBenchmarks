using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using AwesomeAssertions;

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

// 4. TodoListBase Component Code-Behind
public abstract class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

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
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var newItem = await TodoService.AddAsync(NewTitle);

        // Update local state and reload list
        Todos.Add(newItem);
        NewTitle = "";
        await LoadTodos();
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

// 6. xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectIdAndTitle()
    {
        var service = new TodoService();

        var result = await service.AddAsync("Buy groceries");

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("Buy groceries");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllItems()
    {
        var service = new TodoService();
        await service.AddAsync("Task 1");
        await service.AddAsync("Task 2");

        var todos = await service.GetAllAsync();

        todos.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Item to delete");
        await service.AddAsync("Item to keep");

        var todos = await service.GetAllAsync();
        var itemIdToDelete = todos[0].Id;

        await service.DeleteAsync(itemIdToDelete);

        var updatedTodos = await service.GetAllAsync();

        updatedTodos.Should().HaveCount(1);
        updatedTodos.Should().NotContain(todos.First(t => t.Id == itemIdToDelete));
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlag()
    {
        var service = new TodoService();
        await service.AddAsync("Toggle me");

        var todos = await service.GetAllAsync();
        var itemId = todos[0].Id;

        // Initial state
        todos[0].IsCompleted.Should().Be(false);

        await service.ToggleAsync(itemId);

        // New state
        todos[0].IsCompleted.Should().Be(true);
    }
}

// 7. bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_LoadsTodosFromService()
    {
        // Arrange
        var mockService = Substitute.For<ITodoService>();
        var expectedTodos = new List<TodoItem>
        {
            new TodoItem(1, "Test 1", false),
            new TodoItem(2, "Test 2", false)
        };

        // Configure the mock to return data when GetAllAsync is called
        mockService.GetAllAsync().Returns(expectedTodos);

        // Inject the mock into the component
        var component = new TodoListBase();
        component.TodoService = mockService;

        // Act
        await component.RenderAsync();

        // Assert
        // Verify that the component called GetAllAsync upon initialization
        await mockService.Received(1).GetAllAsync();

        // Verify the component state was populated correctly
        component.Todos.Should().HaveCount(2);
        component.Todos[0].Title.Should().Be("Test 1");
    }
}