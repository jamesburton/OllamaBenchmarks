using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
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

// 4. TodoListBase Component Logic
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
        await LoadTodos();
        NewTitle = "";
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
        addedItem.Should().BeEquivalentTo(new TodoItem(3, title, false)); // ID starts at 3 due to seeding
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = new TodoService();
        var initialCount = (await service.GetAllAsync()).Count;
        var idToDelete = 1;

        // Act
        await service.DeleteAsync(idToDelete);

        // Assert
        var todos = await service.GetAllAsync();
        todos.Should().NotContain(t => t.Id == idToDelete);
        todos.Count.Should().Be(initialCount - 1);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        // Arrange
        var service = new TodoService();
        var idToToggle = 2; // Seeded as completed (true)

        // Act 1: Toggle it (true -> false)
        await service.ToggleAsync(idToToggle);
        var todosAfterFirstToggle = await service.GetAllAsync();
        var itemAfterFirstToggle = todosAfterFirstToggle.First(t => t.Id == idToToggle);
        itemAfterFirstToggle.IsCompleted.Should().BeFalse();

        // Act 2: Toggle it again (false -> true)
        await service.ToggleAsync(idToToggle);
        var todosAfterSecondToggle = await service.GetAllAsync();
        var itemAfterSecondToggle = todosAfterSecondToggle.First(t => t.Id == idToToggle);
        itemAfterSecondToggle.IsCompleted.Should().BeTrue();
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests : BunitTestContext
{
    [Fact]
    public async Task TodoListBase_InitializesAndLoadsTodos()
    {
        // Arrange
        var mockService = Substitute.For<ITodoService>();
        var initialTodos = new List<TodoItem>
        {
            new TodoItem(10, "Initial", false),
            new TodoItem(20, "Another", true)
        };

        // Configure the mock service to return the initial list
        mockService.GetAllAsync().Returns(Task.FromResult(initialTodos));

        // Act
        // Inject the mock service into the component context
        var cut = RenderComponent<TodoListBase>(parameters =>
        {
            // Manually inject the mock service into the component instance
            cut.Instance.TodoService = mockService;
        });

        // Wait for OnInitializedAsync to complete (which calls GetAllAsync)
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert 1: Verify the service method was called on initialization
        await mockService.Received(1).GetAllAsync();

        // Assert 2: Verify the component rendered the correct initial data
        cut.Markup.Should().Contain("Initial");
        cut.Markup.Should().Contain("Another");
    }
}