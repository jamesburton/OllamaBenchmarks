using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using NSubstitute;
using Bunit;
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

    public TodoService()
    {
        // Seed initial data for testing
        _todos.Add(new TodoItem(1, "Buy groceries", false));
        _todos.Add(new TodoItem(2, "Finish report", true));
        _nextId = 3;
    }

    public Task<List<TodoItem>> GetAllAsync()
    {
        // Return a copy to prevent external modification
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
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos.Remove(item);
        }
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            // Create a new record instance with the flipped state
            var updatedItem = item with { IsCompleted = !item.IsCompleted };

            // Find and replace the item in the list (since records are immutable)
            var index = _todos.FindIndex(t => t.Id == id);
            if (index != -1)
            {
                _todos[index] = updatedItem;
            }
        }
        return Task.CompletedTask;
    }
}

// 4. TodoListBase Component
public partial class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadTodos();
    }

    public async Task LoadTodos()
    {
        Todos = await TodoService.GetAllAsync().Result.ToList();
        StateHasChanged();
    }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        await TodoService.AddAsync(NewTitle);
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

// 5. xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        // Arrange
        var service = new TodoService();
        var title = "New Test Item";

        // Act
        var result = await service.AddAsync(title);

        // Assert
        result.Should().BeOfType<TodoItem>().Subject.Should().HaveProperty(t => t.Title).WithValue(title);
        // Check if the ID was correctly assigned (it should be the next available ID)
        // Since the service starts with 2 items, the first added item should have ID 3.
        result.Should().HaveProperty(t => t.Id).WithValue(3); 
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = new TodoService();
        // The initial list has IDs 1 and 2. We will delete ID 1.
        int idToDelete = 1;

        // Act
        await service.DeleteAsync(idToDelete);

        // Assert
        var todos = await service.GetAllAsync();
        todos.Should().NotContain(t => t.Id == idToDelete);
        todos.Should().HaveCount(1); // Only ID 2 should remain
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        // Arrange
        var service = new TodoService();
        // Item 1: "Buy groceries", IsCompleted = false
        int idToToggle = 1;

        // Act 1: Toggle it (false -> true)
        await service.ToggleAsync(idToToggle);

        // Assert 1
        var todosAfterFirstToggle = await service.GetAllAsync();
        var itemAfterFirstToggle = todosAfterFirstToggle.First(t => t.Id == idToToggle);
        itemAfterFirstToggle.IsCompleted.Should().BeTrue();

        // Act 2: Toggle it again (true -> false)
        await service.ToggleAsync(idToToggle);

        // Assert 2
        var todosAfterSecondToggle = await service.GetAllAsync();
        var itemAfterSecondToggle = todosAfterSecondToggle.First(t => t.Id == idToToggle);
        itemAfterSecondToggle.IsCompleted.Should().BeFalse();
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests : TestContext
{
    [Fact]
    public async Task TodoListBase_LoadsTodosOnInitialization()
    {
        // Arrange
        // Mock the service using NSubstitute
        var mockService = Substitute.For<ITodoService>();

        // Define the initial state returned by the service
        var initialTodos = new List<TodoItem>
        {
            new TodoItem(10, "Mock Task A", false),
            new TodoItem(20, "Mock Task B", true)
        };

        // Configure the mock to return the initial list when GetAllAsync is called
        mockService.GetAllAsync().Returns(Task.FromResult(initialTodos));

        // Setup the component context
        var cut = RenderComponent<TodoListBase>(parameters =>
        {
            // Inject the mock service into the component instance
            parameters.Add(p => p.TodoService, mockService);
        });

        // Act: The component initializes and calls OnInitializedAsync, triggering GetAllAsync
        await Task.Delay(10); // Wait for async initialization to complete

        // Assert 1: Verify the service method was called
        await mockService.Received(1).GetAllAsync();

        // Assert 2: Verify the component rendered the correct initial data
        cut.Markup.Should().Contain("Mock Task A");
        cut.Markup.Should().Contain("Mock Task B");
    }
}