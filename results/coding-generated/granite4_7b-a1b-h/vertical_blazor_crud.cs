using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AwesomeAssertions;
using NSubstitute;
using Bunit;

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
    private static int _nextId = 1;
    private readonly List<TodoItem> _todos = new();

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.AsReadOnly());

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        var index = _todos.FindIndex(t => t.Id == id);
        if (index != -1)
        {
            _todos.RemoveAt(index);
        }
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var index = _todos.FindIndex(t => t.Id == id);
        if (index != -1)
        {
            _todos[index] = _todos[index].WithIsCompleted(!(_todos[index].IsCompleted));
        }
        return Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = null!;

    public List<TodoItem> Todos { get; set; } = new();

    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        Todos.Add(await TodoService.AddAsync(NewTitle));
        NewTitle = "";
        await InvokeAsync(StateHasChanged);
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

public class TodoListBaseTests
{
    [Fact]
    public async Task AddTodo_AddsNewItem()
    {
        // Arrange
        var ctx = new TestContext();
        var cut = ctx.RenderComponent<TodoListBase>();

        // Act
        cut.Find("input").SetValue("Buy groceries");
        cut.Find("button").Click();
        var todos = await cut.Instance.GetAllTodosAsync();

        // Assert
        Assert.Equal(1, todos.Count);
        Assert.Equal("Buy groceries", todos[0].Title);
        Assert.False(todos[0].IsCompleted);
    }

    [Fact]
    public async Task DeleteTodo_RemovesItem()
    {
        // Arrange
        var ctx = new TestContext();
        var cut = ctx.RenderComponent<TodoListBase>();
        var item = await cut.Instance.AddTodoAsync("Test item");
        var todos = await cut.Instance.GetAllTodosAsync();

        // Act
        cut.Find($"[data-id='{item.Id}'].delete").Click();
        var updatedTodos = await cut.Instance.GetAllTodosAsync();

        // Assert
        Assert.Equal(todos.Count - 1, updatedTodos.Count);
        Assert.DoesNotContain(updatedTodos, t => t.Id == item.Id);
    }

    [Fact]
    public async Task ToggleTodo_TogglesCompleted()
    {
        // Arrange
        var ctx = new TestContext();
        var cut = ctx.RenderComponent<TodoListBase>();
        var item = await cut.Instance.AddTodoAsync("Test item");
        var todos = await cut.Instance.GetAllTodosAsync();

        // Act
        cut.Find($"[data-id='{item.Id}'].toggle").Click();
        var updatedTodos = await cut.Instance.GetAllTodosAsync();

        // Assert
        Assert.NotEqual(todos[0].IsCompleted, updatedTodos[0].IsCompleted);
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsTodos()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var ctx = new TestContext();
        var cut = ctx.RenderComponent<TodoListBase>();
        cut.Instance.TodoService = service;

        // Act
        service.GetAllAsync().Returns(new List<TodoItem>
        {
            new(1, "Buy groceries", false),
            new(2, "Read book", false)
        });

        // Assert
        var todos = await cut.Instance.GetAllTodosAsync();
        Assert.Equal(2, todos.Count);
        Assert.Equal("Buy groceries", todos[0].Title);
        Assert.Equal("Read book", todos[1].Title);
    }

    [Fact]
    public async Task AddAsync_AddsNewItem()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var ctx = new TestContext();
        var cut = ctx.RenderComponent<TodoListBase>();
        cut.Instance.TodoService = service;

        // Act
        service.AddAsync("Test item").Returns(new TodoItem(1, "Test item", false));

        // Assert
        var todos = await cut.Instance.GetAllTodosAsync();
        Assert.Equal(1, todos.Count);
        Assert.Equal("Test item", todos[0].Title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var ctx = new TestContext();
        var cut = ctx.RenderComponent<TodoListBase>();
        cut.Instance.TodoService = service;
        var item = await cut.Instance.AddTodoAsync("Test item");
        var todos = await cut.Instance.GetAllTodosAsync();

        // Act
        service.DeleteAsync(item.Id).WithoutException();
        var updatedTodos = await cut.Instance.GetAllTodosAsync();

        // Assert
        Assert.Equal(todos.Count - 1, updatedTodos.Count);
        Assert.DoesNotContain(updatedTodos, t => t.Id == item.Id);
    }

    [Fact]
    public async Task ToggleAsync_TogglesCompleted()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var ctx = new TestContext();
        var cut = ctx.RenderComponent<TodoListBase>();
        cut.Instance.TodoService = service;
        var item = await cut.Instance.AddTodoAsync("Test item");
        var todos = await cut.Instance.GetAllTodosAsync();

        // Act
        service.ToggleAsync(item.Id).WithoutException();
        var updatedTodos = await cut.Instance.GetAllTodosAsync();

        // Assert
        Assert.NotEqual(todos[0].IsCompleted, updatedTodos[0].IsCompleted);
    }
}