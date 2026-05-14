using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using NSubstitute;
using NSubstitute.Hooks;
using NSubstitute.Helper;
using Bunit;
using AwesomeAssertions;
using Xunit;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
}

public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

public class TodoService : ITodoService
{
    private List<TodoItem> _items = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(new List<TodoItem>(_items));

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem { Id = _nextId++, Title = title, IsCompleted = false };
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        _items.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        item.IsCompleted = !item.IsCompleted;
        return Task.CompletedTask;
    }
}

public partial class TodoListBase : ComponentBase
{
    public [Inject] public ITodoService TodoService { get; set; }
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";
    private readonly BrowserUrlBuilder _urlBuilder = new();

    protected override async Task OnInitializedAsync()
    {
        reloadTodos();
    }

    public async Task AddTodo()
    {
        await TodoService!.AddAsync(NewTitle);
        NewTitle = "";
        reloadTodos();
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        reloadTodos();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        reloadTodos();
    }

    private async Task reloadTodos()
    {
        Todos = await TodoService.GetAllAsync();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task AddTodo_CreatesItemWithCorrectTitle()
    {
        var todoService = Substitute.For<ITodoService>();
        var component = new TodoListBase { TodoService = todoService };
        component.NewTitle = "Test Todo";

        await component.AddTodo();

        todoService.AddAsync("Test Todo").ShouldBeInvoked();

        var todos = (List<TodoItem>)TodoService.GetAllAsync().Result;
        todos.Should().ContainSingle().With(t => t.Title).Equals("Test Todo");
    }

    [Fact]
    public async Task DeleteTodo_RemovesItem()
    {
        var todos = new List<TodoItem>
        {
            new TodoItem { Id = 1, Title = "Old", IsCompleted = false },
            new TodoItem { Id = 2, Title = "New", IsCompleted = false }
        };

        var newTodos = new List<TodoItem>
        {
            new TodoItem { Id = 1, Title = "Old", IsCompleted = false }
        };

        var todoService = Substitute.For<ITodoService>();
        var component = new TodoListBase { TodoService = todoService, Todos = todos };

        await component.DeleteTodo(2);

        todoService.DeleteAsync(2).ShouldBeInvoked();

        Console.WriteLine(((List<TodoItem>)Todos).GetEnumerator().Current?.Title ?? "No output");

        Todos.Should().Be(newTodos);
    }

    [Fact]
    public async Task ToggleTodo_FlipsIsCompleted()
    {
        var todo = new TodoItem { Id = 1, Title = "Toggle", IsCompleted = false };
        var todoService = Substitute.For<ITodoService>();

        var newTodo = new TodoItem { Id = 1, Title = "Toggle", IsCompleted = true };
        var component = new TodoListBase { TodoService = todoService };

        await component.ToggleTodo(1);

        // Verify toggle called
        todoService.ToggleAsync(1).ShouldBeInvoked();

        // Verify data reflected
        (Todos.Find(todo) ?? throw new Exception("Todo not found")).IsCompleted.Should().BeTrue();
    }
}