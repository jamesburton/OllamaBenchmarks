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
    private List<TodoItem> _todos = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        // Returns a copy of the list
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

// 4. TodoListBase : ComponentBase code-behind class
public class TodoListBase : ComponentBase
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

// 5. xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitleAndId()
    {
        var service = new TodoService();
        var result = await service.AddAsync("Buy groceries");

        Assert.Equal("Buy groceries", result.Title);
        Assert.Equal(1, result.Id); // Since it starts from 1
        Assert.Single(await service.GetAllAsync());
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Item to delete");
        await service.AddAsync("Another item");

        var allItems = await service.GetAllAsync();
        var itemToDelete = allItems.First(t => t.Title == "Item to delete");

        await service.DeleteAsync(itemToDelete.Id);

        var remainingItems = await service.GetAllAsync();

        Assert.Single(remainingItems);
        Assert.DoesNotContain(remainingItems, t => t.Id == itemToDelete.Id);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlag()
    {
        var service = new TodoService();
        await service.AddAsync("Toggle test");

        var allItems = await service.GetAllAsync();
        var item = allItems.First();

        // Initial state
        Assert.False(item.IsCompleted);

        await service.ToggleAsync(item.Id);

        // Toggled state
        var updatedItems = await service.GetAllAsync();
        var updatedItem = updatedItems.First();

        Assert.True(updatedItem.IsCompleted);
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_LoadsTodosFromService()
    {
        // Arrange
        var mockService = Substitute.For<ITodoService>();
        var expectedTodos = new List<TodoItem>
        {
            new TodoItem(1, "Task A", false),
            new TodoItem(2, "Task B", false)
        };

        // Configure the mock to return data when GetAllAsync is called
        mockService.GetAllAsync().Returns(expectedTodos);

        // Setup the component under test
        var component = new TodoListBase();
        component.TodoService = mockService;

        // Act
        await component.RenderAsync();

        // Assert
        // Verify that GetAllAsync was called exactly once during initialization
        await mockService.Received(1).GetAllAsync();

        // Verify that the component state reflects the loaded data
        Assert.Equal(2, component.Todos.Count);
        Assert.Equal("Task A", component.Todos[0].Title);
    }

    [Fact]
    public async Task AddTodo_CallsServiceAndUpdatesList()
    {
        // Arrange
        var mockService = Substitute.For<ITodoService>();
        var newTodo = new TodoItem(99, "New Task", false);

        // Setup AddAsync to return the newly created item
        mockService.AddAsync("New Task").Returns(newTodo);
        mockService.GetAllAsync().Returns(new List<TodoItem>()); // Ensure initial load is empty

        var component = new TodoListBase();
        component.TodoService = mockService;

        // Act
        await component.AddTodo();

        // Assert
        // Verify that AddAsync was called with the correct title
        await mockService.Received(1).AddAsync("New Task");

        // Verify that the component's internal list was updated (requires checking the component's state)
        Assert.Single(component.Todos);
        Assert.Equal("New Task", component.Todos[0].Title);
    }
}