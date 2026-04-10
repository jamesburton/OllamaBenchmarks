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
public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;

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

// --- Component Markup (Required for bUnit testing context) ---
// Note: In a real scenario, this would be in a .razor file.
public partial class TodoListBase : ComponentBase
{
    private string RenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "todo-list-container");

        // Input form
        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "input-group");
        builder.OpenElement(4, "input");
        builder.AddAttribute(5, "type", "text");
        builder.AddAttribute(6, "value", newobject[] { NewTitle });
        builder.CloseElement(); // input

        builder.OpenElement(7, "button");
        builder.AddAttribute(8, "class", "add-button");
        builder.AddAttribute(9, "onclick", EventCallback.Factory.Create(this, async () => await AddTodo()));
        builder.AddContent(10, "Add Todo");
        builder.CloseElement(); // button
        builder.CloseElement(); // input-group

        // Todo list
        builder.OpenElement(11, "ul");
        builder.AddAttribute(12, "class", "todo-list");

        foreach (var todo in Todos)
        {
            builder.OpenElement(13, "li");
            builder.AddAttribute(14, "class", todo.IsCompleted ? "completed" : "");

            // Checkbox/Toggle
            builder.OpenElement(15, "input");
            builder.AddAttribute(16, "type", "checkbox");
            builder.AddAttribute(17, "checked", todo.IsCompleted);
            builder.AddAttribute(18, "onclick", EventCallback.Factory.Create(this, async () => await ToggleTodo(todo.Id)));
            builder.CloseElement(); // input

            // Title
            builder.OpenElement(19, "span");
            builder.AddAttribute(20, "style", todo.IsCompleted ? "text-decoration: line-through;" : "");
            builder.AddContent(21, todo.Title);
            builder.CloseElement(); // span

            // Delete Button
            builder.OpenElement(22, "button");
            builder.AddAttribute(23, "class", "delete-button");
            builder.AddAttribute(24, "onclick", EventCallback.Factory.Create(this, async () => await DeleteTodo(todo.Id)));
            builder.AddContent(25, "X");
            builder.CloseElement(); // button

            builder.CloseElement(); // li
        }

        builder.CloseElement(); // ul
        builder.CloseElement(); // div
        builder.CloseElement(); // div

        return builder.Render();
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
        var result = await service.AddAsync(title);

        // Assert
        result.Should().BeAssignableTo<TodoItem>();
        result.Should().BeEquivalentTo(new TodoItem(3, title, false)); // ID should be 3 because 1 and 2 were seeded
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = new TodoService();
        var initialTodos = await service.GetAllAsync();
        var idToDelete = 1;

        // Act
        await service.DeleteAsync(idToDelete);
        var updatedTodos = await service.GetAllAsync();

        // Assert
        updatedTodos.Should().NotContain(t => t.Id == idToDelete);
        updatedTodos.Should().HaveCount(1);
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

        // Assert 1
        itemAfterFirstToggle.IsCompleted.Should().BeFalse();

        // Act 2: Toggle it again (false -> true)
        await service.ToggleAsync(idToToggle);
        var todosAfterSecondToggle = await service.GetAllAsync();
        var itemAfterSecondToggle = todosAfterSecondToggle.First(t => t.Id == idToToggle);

        // Assert 2
        itemAfterSecondToggle.IsCompleted.Should().BeTrue();
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests : TestContext
{
    [Fact]
    public async Task TodoListBase_InitializesAndCallsGetAllAsync()
    {
        // Arrange
        // 1. Mock the service
        var mockService = Substitute.For<ITodoService>();

        // 2. Define the expected initial state
        var initialTodos = new List<TodoItem>
        {
            new TodoItem(10, "Initial Task", false),
            new TodoItem(11, "Completed Task", true)
        };

        // 3. Configure the mock to return the initial state when GetAllAsync is called
        mockService.GetAllAsync().Returns(Task.FromResult(initialTodos));

        // 4. Create the component instance, injecting the mock service
        var cut = RenderComponent<TodoListBase>(() =>
        {
            // Manually inject the mock service into the component instance
            // This simulates the framework's dependency injection process
            var component = new TodoListBase();
            typeof(TodoListBase).GetProperty("TodoService", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(component, mockService);
            return component;
        });

        // Act: Wait for OnInitializedAsync to complete (which calls GetAllAsync)
        await Task.Delay(10); 

        // Assert 1: Verify the service method was called exactly once
        mockService.Received(1).GetAllAsync();

        // Assert 2: Verify the rendered markup contains the initial data
        cut.Markup.Should().Contain("Initial Task");
        cut.Markup.Should().Contain("Completed Task");

        // Assert 3: Verify the structure for the completed task (line-through style)
        cut.Markup.Should().Contain("style=\"text-decoration: line-through;\"");
    }
}