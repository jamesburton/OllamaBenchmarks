using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoApp;

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
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        await Task.CompletedTask;
        return new List<TodoItem>(_items);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        await Task.CompletedTask;
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        await Task.CompletedTask;
        _items.RemoveAll(i => i.Id == id);
    }

    public async Task ToggleAsync(int id)
    {
        await Task.CompletedTask;
        var item = _items.Find(i => i.Id == id);
        if (item != null)
        {
            item = new TodoItem(item.Id, item.Title, !item.IsCompleted);
            _items.Remove(item);
            _items.Add(item);
        }
    }
}

public partial class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = null!;
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
            var todo = await TodoService.AddAsync(NewTitle);
            Todos.Add(todo);
            NewTitle = "";
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos.RemoveAll(t => t.Id == id);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        var todo = Todos.Find(t => t.Id == id);
        if (todo != null)
        {
            todo = new TodoItem(todo.Id, todo.Title, !todo.IsCompleted);
            Todos.Remove(todo);
            Todos.Add(todo);
        }
    }
}

// xUnit tests
public class TodoServiceTests
{
    private readonly TodoService _service = new();

    [Fact]
    public async Task AddAsync_CreatesItem()
    {
        var todo = await _service.AddAsync("Test");
        todo.Should().NotBeNull();
        todo.Title.Should().Be("Test");
        todo.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var todo = await _service.AddAsync("Test");
        await _service.DeleteAsync(todo.Id);
        var todos = await _service.GetAllAsync();
        todos.Should().NotContain(t => t.Id == todo.Id);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var todo = await _service.AddAsync("Test");
        await _service.ToggleAsync(todo.Id);
        var todos = await _service.GetAllAsync();
        var updated = todos.Find(t => t.Id == todo.Id);
        updated.Should().NotBeNull();
        updated.IsCompleted.Should().BeTrue();
    }
}

// bUnit test
public class TodoListBaseTests
{
    [Fact]
    public void RendersAndCallsGetAllAsync()
    {
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));
        var cut = new TestContext().RenderComponent<TodoListBase>(parameters => parameters.Add(p => p.TodoService, service));
        service.Received(1).GetAllAsync();
    }
}