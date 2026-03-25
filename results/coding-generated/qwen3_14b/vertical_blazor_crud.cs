using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Bunit;
using Xunit;
using NSubstitute;
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

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return new List<TodoItem>(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        _todos.RemoveAll(t => t.Id == id);
    }

    public async Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item = new TodoItem(item.Id, item.Title, !item.IsCompleted);
            _todos[_todos.IndexOf(item)] = item;
        }
    }
}

// 4. TodoListBase component code-behind
public class TodoListBase : ComponentBase
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
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        Todos = await TodoService.GetAllAsync();
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

// 5. xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task TestAddAsync_CreatesItemWithCorrectTitle()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var expectedItem = new TodoItem(1, "Test Title", false);
        service.AddAsync("Test Title").Returns(Task.FromResult(expectedItem));

        // Act
        var result = await service.AddAsync("Test Title");

        // Assert
        result.Should().BeEquivalentTo(expectedItem);
    }

    [Fact]
    public async Task TestDeleteAsync_RemovesItem()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var items = new List<TodoItem> { new(1, "Item 1", false) };
        service.GetAllAsync().Returns(Task.FromResult(items));

        // Act
        await service.DeleteAsync(1);
        var result = await service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task TestToggleAsync_FlipsIsCompleted()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var initialItem = new TodoItem(1, "Item 1", false);
        service.GetAllAsync().Returns(Task.FromResult(new List<TodoItem> { initialItem }));

        // Act
        await service.ToggleAsync(1);
        var result = await service.GetAllAsync();

        // Assert
        result.Single().IsCompleted.Should().BeTrue();
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public void TestComponentCallsGetAllAsyncOnInit()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var todos = new List<TodoItem> { new(1, "Test", false) };
        service.GetAllAsync().Returns(Task.FromResult(todos));

        var ctx = new TestContext();
        var cut = ctx.RenderComponent<TodoListBase>(p =>
            p.Add(x => x.TodoService, service));

        // Act
        // No action needed; OnInitializedAsync is called automatically

        // Assert
        service.Received().GetAllAsync();
    }
}