using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using Xunit;
using Bunit;
using FluentAssertions;
using FluentAssertions.Execution;

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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(new List<TodoItem>(_todos));

    public Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return Task.FromResult(todo);
    }

    public Task DeleteAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) _todos.Remove(todo);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) todo.IsCompleted = !todo.IsCompleted;
        return Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = (await TodoService.GetAllAsync()) ?? new();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            Todos = (await TodoService.GetAllAsync()) ?? new();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos = (await TodoService.GetAllAsync()) ?? new();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        Todos = (await TodoService.GetAllAsync()) ?? new();
    }
}

public class TodoServiceTests
{
    private readonly TodoService _service = new();

    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        // Act
        var result = await _service.AddAsync("Test Todo");

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Todo");
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var todo = await _service.AddAsync("Todo to delete");

        // Act
        await _service.DeleteAsync(todo.Id);

        // Assert
        var allTodos = await _service.GetAllAsync();
        allTodos.Should().NotContain(t => t.Id == todo.Id);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        // Arrange
        var todo = await _service.AddAsync("Toggleable Todo");
        todo.IsCompleted.Should().BeFalse();

        // Act
        await _service.ToggleAsync(todo.Id);

        // Assert
        var updatedTodo = (await _service.GetAllAsync()).First(t => t.Id == todo.Id);
        updatedTodo.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        // Arrange
        var ctx = new TestContext();
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem> { new(1, "Test", false) });

        var cut = ctx.RenderComponent<TodoListBase>(p =>
            p.Add(x => x.TodoService, service));

        // Act
        await cut.WaitForNextRender();

        // Assert
        await service.Received(1).GetAllAsync();
        cut.Instance.Todos.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddTodo_CallsAddAsyncAndReloadsList()
    {
        // Arrange
        var ctx = new TestContext();
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem>());
        service.AddAsync(Arg.Any<string>()).Returns(new TodoItem(1, "New Todo", false));

        var cut = ctx.RenderComponent<TodoListBase>(p =>
            p.Add(x => x.TodoService, service));

        cut.Instance.NewTitle = "New Todo";

        // Act
        await cut.Instance.AddTodo();

        // Assert
        await service.Received(1).AddAsync("New Todo");
        await service.Received(2).GetAllAsync();
        cut.Instance.NewTitle.Should().BeEmpty();
    }
}