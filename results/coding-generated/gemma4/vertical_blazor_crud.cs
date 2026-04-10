using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
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
            // Since TodoItem is a record, we must replace it to change state in the list
            var updatedItem = item with { IsCompleted = !item.IsCompleted };
            var index = _todos.IndexOf(item);
            if (index != -1)
            {
                _todos[index] = updatedItem;
            }
        }
        return Task.CompletedTask;
    }
}

// 4. TodoListBase : ComponentBase code-behind class
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

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        await LoadTodos();
        StateHasChanged();
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadTodos();
        StateHasChanged();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadTodos();
        StateHasChanged();
    }

    private async Task LoadTodos()
    {
        Todos = (await TodoService.GetAllAsync()).ToList();
    }
}

// --- Component Placeholder (Required for bUnit testing context) ---
// In a real scenario, this would be in a .razor file.
public class TodoList : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadTodos();
    }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        await LoadTodos();
        StateHasChanged();
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadTodos();
        StateHasChanged();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadTodos();
        StateHasChanged();
    }

    private async Task LoadTodos()
    {
        Todos = (await TodoService.GetAllAsync()).ToList();
    }
}

// --- xUnit Tests for TodoService ---
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        // Arrange
        var service = new TodoService();
        var title = "Buy Milk";

        // Act
        var result = await service.AddAsync(title);

        // Assert
        result.Should().BeEquivalentTo(new TodoItem(1, title, false));
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = new TodoService();
        await service.AddAsync("Item 1");
        await service.AddAsync("Item 2"); // ID 2

        // Act
        await service.DeleteAsync(2);

        // Assert
        var todos = await service.GetAllAsync();
        todos.Should().HaveCount(1);
        todos.First().Title.Should().Be("Item 1");
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        // Arrange
        var service = new TodoService();
        await service.AddAsync("To Toggle"); // ID 1, IsCompleted = false

        // Act 1: Toggle to true
        await service.ToggleAsync(1);
        var todosAfterFirstToggle = await service.GetAllAsync();
        var itemAfterFirstToggle = todosAfterFirstToggle.First(t => t.Id == 1);
        itemAfterFirstToggle.IsCompleted.Should().BeTrue();

        // Act 2: Toggle back to false
        await service.ToggleAsync(1);
        var todosAfterSecondToggle = await service.GetAllAsync();
        var itemAfterSecondToggle = todosAfterSecondToggle.First(t => t.Id == 1);
        itemAfterSecondToggle.IsCompleted.Should().BeFalse();
    }
}

// --- bUnit Test for TodoListBase ---
public class TodoListBaseTests : TestContext
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsyncOnService()
    {
        // Arrange
        var mockService = Substitute.For<ITodoService>();
        var initialTodos = new List<TodoItem>
        {
            new TodoItem(1, "Initial", false),
            new TodoItem(2, "Another", true)
        };

        // Configure the mock to return our initial state
        mockService.GetAllAsync().Returns(initialTodos);

        // Setup the component instance, injecting the mock service
        var component = new TodoListBase();
        component.TodoService = mockService;

        // Act
        await component.OnInitializedAsync();

        // Assert
        // Verify that the service method was called exactly once during initialization
        await mockService.Received(1).GetAllAsync();

        // Verify the component state was updated
        component.Todos.Should().BeEquivalentTo(initialTodos);
    }
}