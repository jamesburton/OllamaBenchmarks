using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using NSubstitute;
using Bunit;
using System;
using System.Collections.ObjectModel;

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

// 4. TodoListBase Component code-behind
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
        Todos = (await TodoService.GetAllAsync()).ToList();
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
        var title = "New test item";

        // Act
        var addedItem = await service.AddAsync(title);

        // Assert
        addedItem.Title.Should().Be(title);
        addedItem.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = new TodoService();
        // Initial state check (assuming ID 1 exists)
        var initialTodos = await service.GetAllAsync();
        initialTodos.Count.Should().BeGreaterThan(0);

        // Act
        await service.DeleteAsync(1);

        // Assert
        var remainingTodos = await service.GetAllAsync();
        remainingTodos.Any(t => t.Id == 1).Should().BeFalse();
        remainingTodos.Count.Should().Be(initialTodos.Count - 1);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        // Arrange
        var service = new TodoService();
        // Assuming ID 2 exists and is completed (true)
        var initialTodos = await service.GetAllAsync();
        var itemToToggle = initialTodos.FirstOrDefault(t => t.Id == 2);

        if (itemToToggle == null)
        {
            // Skip test if setup data changes
            return;
        }

        // Act
        await service.ToggleAsync(2);

        // Assert
        var updatedTodos = await service.GetAllAsync();
        var toggledItem = updatedTodos.First(t => t.Id == 2);
        toggledItem.IsCompleted.Should().BeFalse(); // Should flip from true to false
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests : TestContext
{
    [Fact]
    public async Task TodoListBase_InitializesAndLoadsTodos()
    {
        // Arrange
        var mockService = Substitute.For<ITodoService>();
        var expectedTodos = new List<TodoItem>
        {
            new TodoItem(10, "Mock Item A", false),
            new TodoItem(20, "Mock Item B", true)
        };

        // Configure the mock service to return the expected list on initialization
        mockService.GetAllAsync().Returns(Task.FromResult(expectedTodos));

        // Act
        var cut = RenderComponent<TodoListBase>(sk =>
        {
            // Inject the mock service into the component instance
            sk.Add(p => p.TodoService, mockService);
        });

        // Wait for OnInitializedAsync to complete
        await Task.Delay(10); 

        // Assert 1: Verify the service was called on initialization
        await mockService.Received(1).GetAllAsync();

        // Assert 2: Verify the component rendered the correct initial state
        cut.Markup.Should().Contain("Mock Item A");
        cut.Markup.Should().Contain("Mock Item B");
    }
}