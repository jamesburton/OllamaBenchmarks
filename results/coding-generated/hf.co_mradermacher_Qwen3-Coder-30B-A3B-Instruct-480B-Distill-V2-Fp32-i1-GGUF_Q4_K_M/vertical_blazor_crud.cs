using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Xunit;
using Bunit;
using NSubstitute;
using AwesomeAssertions;

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

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(new List<TodoItem>(_todos));
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        _todos.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _todos.Find(x => x.Id == id);
        if (item != null)
        {
            _todos[_todos.IndexOf(item)] = item with { IsCompleted = !item.IsCompleted };
        }
        return Task.CompletedTask;
    }
}

public abstract class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            Todos = await TodoService.GetAllAsync();
        }
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

public class TodoListTests : TodoListBase
{
    // This class is just to satisfy the base class requirements for testing
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test Title");
        item.Title.Should().Be("Test Title");
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test");
        var all = await service.GetAllAsync();
        all.Count.Should().Be(1);

        await service.DeleteAsync(item.Id);
        all = await service.GetAllAsync();
        all.Count.Should().Be(0);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test");
        var initial = item.IsCompleted;

        await service.ToggleAsync(item.Id);
        var toggled = await service.GetAllAsync();
        toggled[0].IsCompleted.Should().NotBe(initial);
    }
}

public class TodoListComponentTests
{
    [Fact]
    public async Task RendersComponentAndCallsGetAllAsync()
    {
        var mockService = Substitute.For<ITodoService>();
        var todos = new List<TodoItem>
        {
            new(1, "Test Item", false)
        };
        mockService.GetAllAsync().Returns(todos);

        var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<TodoListTests>(p =>
            p.Add(x => x.TodoService, mockService));

        mockService.Received().GetAllAsync();

        cut.Markup.Should().Contain("Test Item");
    }
}